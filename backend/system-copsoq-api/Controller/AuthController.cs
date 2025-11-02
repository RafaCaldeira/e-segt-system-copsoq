using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models;
using system_copsoq_api.Services;
using Microsoft.EntityFrameworkCore;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher; 
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher, ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff([FromBody] RegistroStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // --- Validação de Segurança ---
        // Este endpoint NÃO PODE criar Clientes
        if (dto.Role == Role.Cliente)
        {
            return BadRequest("Use o endpoint /register-cliente para registar clientes.");
        }

        // (Opcional: Verifique se o email já existe)
        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return Conflict("Este email já está a ser utilizado.");
        }

        // 1. Criar o novo Usuário (Admin ou Psicologa)
        var novoUsuario = new User
        {
            Email = dto.Email,
            Role = dto.Role,
            EmpresaID = null // Staff não tem EmpresaID
        };

        // 2. Fazer o Hash da senha
        novoUsuario.SenhaHash = _passwordHasher.HashPassword(novoUsuario, dto.Senha);

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync(); // Salva o usuário no banco

        return StatusCode(201, new { Message = "Usuário de staff registado com sucesso!" });
    }

        // POST: api/auth/register-cliente
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegistroClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Criar a Empresa primeiro
            var novaEmpresa = new Empresa
            {
                NomeEmpresa = dto.NomeEmpresa,
                NomeResponsavel = dto.NomeResponsavel,
                SetorAtuacao = dto.SetorAtuacao,
                Cidade = dto.Cidade,
                Cnpj = dto.Cnpj
            };

            _context.Empresas.Add(novaEmpresa);
            // IMPORTANTE: Salvar aqui para que o 'novaEmpresa.ID' seja gerado pelo banco
            await _context.SaveChangesAsync();

            // 2. Criar o Usuário (o Login)
            var novoUsuario = new User
            {
                Email = dto.Email,
                Role = Role.Cliente,
                EmpresaID = novaEmpresa.ID
            };

            // 3. Fazer o Hash da senha
            novoUsuario.SenhaHash = _passwordHasher.HashPassword(novoUsuario, dto.Senha);

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync(); // Salva o usuário no banco

            // Retorna '201 Created'
            return StatusCode(201, new { Message = "Cliente registrado com sucesso!" });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Encontrar o usuário pelo email
            var user = await _context.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Email ou senha inválidos."); // Não diga qual está errado
            }

            // 2. Verificar a senha
            var result = _passwordHasher.VerifyHashedPassword(user, user.SenhaHash, loginDto.Senha);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // 3. Gerar e retornar o token
            var token = _tokenService.CreateToken(user);
            
            return Ok(new 
            {
                Message = "Login bem-sucedido!",
                Token = token,
                UserRole = user.Role.ToString() // Envia a Role para o front-end
            });
        }
    }
}