using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using System.Linq;
using system_copsoq_api.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <-- Só usuários logados podem acessar este controller
    public class FuncionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FuncionarioController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/funcionario
        [HttpPost]
        [Authorize(Roles = "Cliente")] // <-- Apenas Clientes podem registrar funcionários
        public async Task<IActionResult> CreateFuncionario([FromBody] FuncionarioCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Lógica de Segurança Chave ---
            // 1. Pegar o Email do usuário logado (que está no Token JWT)
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userEmail == null)
            {
                return Unauthorized(); // Token inválido ou não encontrado
            }

            // 2. Buscar o usuário no banco para encontrar seu EmpresaID
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null || user.EmpresaID == null)
            {
                // Se não for um usuário válido ou não for um Cliente (não tem EmpresaID)
                return Forbid("Este usuário não está vinculado a nenhuma empresa.");
            }

            var novoFuncionario = new Funcionario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Cargo = dto.Cargo,
                Setor = dto.Setor,
                CPF = dto.CPF,
                EmpresaID = user.EmpresaID.Value // 5. Definir o EmpresaID
            };

            // 3. Forçar o funcionário a pertencer à empresa do usuário logado
            _context.Funcionarios.Add(novoFuncionario);
            await _context.SaveChangesAsync();

            // 6. CORREÇÃO: Retornar o 'novoFuncionario'
            return CreatedAtAction(nameof(GetFuncionario), new { id = novoFuncionario.ID }, novoFuncionario);
        }

        // GET: api/funcionario
        [HttpGet]
        [Authorize(Roles = "Cliente,Administrador")] 
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            var user = await GetUserFromToken();

            if (user == null)
            {
                return Unauthorized("Token inválido.");
            }

            // *** CORREÇÃO (CS0117): Verifique se o nome "Administrador" está correto ***
            if (user.Role == Role.Admin)
            {
                // 1. Admin vê TODOS os funcionários
                return await _context.Funcionarios.ToListAsync();
            }
            else
            {
                // 2. Cliente vê APENAS os seus funcionários
                return await _context.Funcionarios
                    .Where(f => f.EmpresaID == user.EmpresaID)
                    .ToListAsync();
            }
        }

        // GET: api/funcionario/{id}
        // (Este método é usado pelo 'CreatedAtAction' acima)
        [HttpGet("{id}")]
        [Authorize(Roles = "Cliente,Administrador")]
        public async Task<IActionResult> GetFuncionario(int id)
        {
            var user = await GetUserFromToken();
            
            if (user == null)
            {
                return Unauthorized("Token inválido.");
            }
            
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            if (user.Role != Role.Admin && funcionario.EmpresaID != user.EmpresaID)
            {
                return Forbid("Acesso negado a este funcionário.");
            }

            return Ok(funcionario);
        }

        // PUT: api/funcionario/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")] // Só Cliente pode editar
        public async Task<IActionResult> UpdateFuncionario(int id, [FromBody] FuncionarioCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await GetUserFromToken();

            if (user == null)
            {
                return Forbid("Usuário inválido ou não vinculado a uma empresa.");
            }

            var funcionario = await _context.Funcionarios.FindAsync(id);
            
            // 4. Segurança: Cliente só pode editar o seu
            if (funcionario == null)
            {
                return NotFound(); // Se o funcionário não existe, retorne 404
            }

            // Atualiza os dados
            funcionario.Nome = dto.Nome;
            funcionario.Email = dto.Email;
            funcionario.Telefone = dto.Telefone;
            funcionario.Cargo = dto.Cargo;
            funcionario.Setor = dto.Setor;
            funcionario.CPF = dto.CPF;

            await _context.SaveChangesAsync();
            return Ok(funcionario); // Retorna o funcionário atualizado
        }

        // DELETE: api/funcionario/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Cliente,Administrador")] // Cliente ou Admin podem deletar
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var user = await GetUserFromToken();

            if (user == null)
            {
                return Unauthorized("Token inválido.");
            }

            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            if (user.Role != Role.Admin && user.EmpresaID != funcionario.EmpresaID)
            {
                return Forbid("Você não tem permissão para deletar este funcionário.");
            }

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();

            return NoContent(); // Sucesso
        }
        
        // --- Método Auxiliar ---
        // (Este método privado ajuda a não repetir código)
        private async Task<User?> GetUserFromToken()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userEmail == null) return null;

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            
            // Para 'Cliente', EmpresaID é obrigatório. Para 'Admin', não.
            if (user != null && (user.Role == Role.Cliente && user.EmpresaID == null))
            {
                return null; // Cliente inválido sem EmpresaID
            }

            return user;
        }
    }
}