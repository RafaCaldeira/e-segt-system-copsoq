using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models;
using system_copsoq_api.Services;
using Microsoft.EntityFrameworkCore; // <-- Importante para o .Include
using system_copsoq_api.Models.Disparo; // Para o namespace Disparo
using system_copsoq_api.Models.Formularios; // Para o namespace Formularios

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

            // Validação de Segurança (para garantir que só Admin ou Psicologo são criados aqui)
            if (dto.Role == Role.Cliente)
            {
                return BadRequest("Use o endpoint /register-cliente para registar clientes.");
            }
            // (Verifica se o enum é válido, ex: não é um número fora da gama)
             if (!Enum.IsDefined(typeof(Role), dto.Role))
            {
                return BadRequest("Role inválida.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return Conflict("Este email já está a ser utilizado.");
            }

            var novoUsuario = new User
            {
                Email = dto.Email,
                Role = dto.Role,
                EmpresaID = null // Staff não tem EmpresaID
            };

            novoUsuario.SenhaHash = _passwordHasher.HashPassword(novoUsuario, dto.Senha);

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync(); 

            return StatusCode(201, new { Message = "Usuário de staff registado com sucesso!" });
        }

        // POST: api/auth/register-cliente
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegistroClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // (Verifica se o email já existe)
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return Conflict(new { Message = "Este email já está a ser utilizado."});
            }

            var novaEmpresa = new Empresa
            {
                NomeEmpresa = dto.NomeEmpresa,
                NomeResponsavel = dto.NomeResponsavel,
                SetorAtuacao = dto.SetorAtuacao,
                Cidade = dto.Cidade,
                Cnpj = dto.Cnpj,
                IsAtivo = true // Define como ativo por defeito
            };
            
            _context.Empresas.Add(novaEmpresa);
            await _context.SaveChangesAsync(); 

            var novoUsuario = new User
            {
                Email = dto.Email,
                Role = Role.Cliente,
                EmpresaID = novaEmpresa.ID 
            };

            novoUsuario.SenhaHash = _passwordHasher.HashPassword(novoUsuario, dto.Senha);

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync(); 

            return StatusCode(201, new { Message = "Cliente registrado com sucesso!" });
        }
        
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. ATUALIZAÇÃO: Usar .Include(u => u.Empresa)
            // Isto carrega o utilizador E a sua empresa associada (se existir)
            var user = await _context.Usuarios
                .Include(u => u.Empresa) // <-- CARREGA A EMPRESA
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.SenhaHash, loginDto.Senha);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            var token = _tokenService.CreateToken(user);
            
            // 2. ATUALIZAÇÃO: Devolver os novos campos
            return Ok(new 
            {
                Message = "Login bem-sucedido!",
                Token = token,
                UserRole = user.Role.ToString(),
                
                // Se 'user.Empresa' não for nulo, envia o 'NomeEmpresa'
                NomeEmpresa = user.Empresa?.NomeEmpresa, 
                
                // Adicionamos o EmpresaID à resposta do login
                EmpresaId = user.EmpresaID 
            });
        }
    }
}