using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsuarioController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/usuario/5
        [HttpGet("{id}")]
        [Authorize] // Exige estar logado
        public async Task<ActionResult<object>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Retorna apenas os dados seguros
            return new
            {
                id = usuario.ID,
                nome = usuario.Empresa?.NomeResponsavel ?? "Usuário", // Se for staff, ajustamos depois
                email = usuario.Email,
                nomeEmpresa = usuario.Empresa?.NomeEmpresa,
                role = usuario.Role
            };
        }

        // PUT: api/usuario/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] AtualizarUsuarioDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID inconsistente.");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // 1. Atualiza Email
            usuario.Email = dto.Email;

            // 2. Atualiza Senha (se informada)
            if (!string.IsNullOrEmpty(dto.Senha))
            {
                usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);
            }

            // 3. Atualiza dados da Empresa (se for Cliente)
            if (usuario.Role == Role.Cliente && usuario.Empresa != null)
            {
                usuario.Empresa.NomeEmpresa = dto.NomeEmpresa;
                usuario.Empresa.NomeResponsavel = dto.Nome; // Usamos o campo 'Nome' do form para o Responsável
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id)) return NotFound();
                else throw;
            }

            return NoContent(); // Sucesso 204
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.ID == id);
        }
    }

    // DTO simples para receber os dados
    public class AtualizarUsuarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? Senha { get; set; }
    }
}