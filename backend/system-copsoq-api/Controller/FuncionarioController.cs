using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using system_copsoq_api.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class FuncionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FuncionarioController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LISTAR FUNCIONÁRIOS
        // GET: api/funcionario
        [HttpGet]
        [Authorize(Roles = "Cliente,Admin")] // <-- Admin incluído
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return Unauthorized();

            // *** CORREÇÃO AQUI ***
            if (user.Role == Role.Admin)
            {
                // Se for Admin, retorna TODOS os funcionários (para o filtro funcionar)
                return await _context.Funcionarios.ToListAsync();
            }
            else
            {
                // Se for Cliente, retorna apenas os da sua empresa
                return await _context.Funcionarios
                    .Where(f => f.EmpresaID == user.EmpresaID)
                    .ToListAsync();
            }
        }

        // 2. OBTER UM FUNCIONÁRIO (Para editar)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFuncionario(int id)
        {
            var user = await GetUserFromToken();
            if (user == null) return Unauthorized();

            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null) return NotFound();

            if (user.Role == Role.Cliente && funcionario.EmpresaID != user.EmpresaID)
            {
                return Forbid("Acesso negado.");
            }

            return Ok(funcionario);
        }

        // ... (Mantenha os outros métodos Create, Update, Delete, Importar como estavam) ...
        // (Vou omitir para poupar espaço, mas copie do seu ficheiro anterior se precisar)
        
        // 3. CRIAR FUNCIONÁRIO
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CreateFuncionario([FromBody] FuncionarioCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await GetUserFromToken();
            if (user == null || user.EmpresaID == null) return Forbid("Utilizador inválido.");

            if (await _context.Funcionarios.AnyAsync(f => f.Email == dto.Email && f.EmpresaID == user.EmpresaID))
                 return BadRequest("Já existe um funcionário com este email.");

            var novoFuncionario = new Funcionario
            {
                Nome = dto.Nome, Email = dto.Email, Telefone = dto.Telefone, Cargo = dto.Cargo, Setor = dto.Setor, CPF = dto.CPF,
                EmpresaID = user.EmpresaID.Value 
            };
            _context.Funcionarios.Add(novoFuncionario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFuncionario), new { id = novoFuncionario.ID }, novoFuncionario);
        }

        // 4. ATUALIZAR FUNCIONÁRIO
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> UpdateFuncionario(int id, [FromBody] FuncionarioCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await GetUserFromToken();
            if (user == null) return Forbid("Utilizador inválido.");
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return NotFound();
            if (funcionario.EmpresaID != user.EmpresaID) return Forbid("Acesso negado.");

            funcionario.Nome = dto.Nome; funcionario.Email = dto.Email; funcionario.Telefone = dto.Telefone;
            funcionario.Cargo = dto.Cargo; funcionario.Setor = dto.Setor; funcionario.CPF = dto.CPF;

            await _context.SaveChangesAsync();
            return Ok(funcionario);
        }

        // 5. EXCLUIR FUNCIONÁRIO
        [HttpDelete("{id}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var user = await GetUserFromToken();
            if (user == null) return Unauthorized();
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return NotFound();
            if (funcionario.EmpresaID != user.EmpresaID) return Forbid("Acesso negado.");
            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 6. IMPORTAR CSV
        [HttpPost("importar")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ImportarCsv(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Nenhum ficheiro enviado.");
            var user = await GetUserFromToken();
            if (user == null || user.EmpresaID == null) return Forbid("Utilizador inválido.");

            var funcionariosCriados = 0;
            var erros = new List<string>();

            try {
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", HasHeaderRecord = true, MissingFieldFound = null }))
                {
                    var registros = csv.GetRecords<FuncionarioCsvDto>().ToList();
                    foreach (var item in registros) {
                        if (string.IsNullOrWhiteSpace(item.Nome) || string.IsNullOrWhiteSpace(item.Email)) { erros.Add($"Ignorado: {item.Nome}"); continue; }
                        bool jaExiste = await _context.Funcionarios.AnyAsync(f => f.Email == item.Email && f.EmpresaID == user.EmpresaID);
                        if (jaExiste) { erros.Add($"Email duplicado: {item.Email}"); continue; }

                        var novoFunc = new Funcionario { Nome = item.Nome, Email = item.Email, Telefone = item.Telefone, Cargo = item.Cargo, Setor = item.Setor, CPF = item.CPF, EmpresaID = user.EmpresaID.Value };
                        _context.Funcionarios.Add(novoFunc);
                        funcionariosCriados++;
                    }
                    await _context.SaveChangesAsync();
                }
            } catch (Exception ex) { return BadRequest($"Erro no CSV: {ex.Message}"); }
            return Ok(new { Message = $"{funcionariosCriados} importados.", Erros = erros });
        }

        private async Task<User?> GetUserFromToken()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userEmail == null) return null;
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        [HttpGet("empresa/{empresaId}/status")]
        [Authorize(Roles = "Psicologo")]
        public async Task<IActionResult> GetFuncionariosComStatus(int empresaId)
        {
            var funcionarios = await _context.Funcionarios
                .Where(f => f.EmpresaID == empresaId)
                .Select(f => new {
                    Id = f.ID,
                    NomeCompleto = f.Nome,
                    Email = f.Email,
                    
                    JaRespondeu = _context.RespostasFuncionarios
                    .Any(r => r.Disparo.FuncionarioID == f.ID)
                })
                .ToListAsync();

            return Ok(funcionarios);
        }

        [Authorize(Roles = "Psicologo,Admin")]
        [HttpGet("empresa/{empresaId}/funcionarios")]
        public async Task<IActionResult> GetFuncionariosPorEmpresa(int empresaId)
        {
            var funcionarios = await _context.Funcionarios
                .Where(f => f.EmpresaID == empresaId)
                .Select(f => new {
                    Id = f.ID,
                    Nome = f.Nome,
                    Email = f.Email,
                    JaRespondeu = _context.RespostasFuncionarios
                        .Any(r => r.Disparo.FuncionarioID == f.ID)
                })
                .ToListAsync();

            return Ok(funcionarios);
        }

        [Authorize(Roles = "Psicologo,Admin")]
        [HttpGet("funcionario/{funcionarioId}/disparos")]
        public async Task<IActionResult> GetDisparosDoFuncionario(int funcionarioId)
        {
            var disparos = await _context.Disparos
                .Where(d => d.FuncionarioID == funcionarioId)
                .Select(d => new {
                    d.ID,
                    d.DataEnvio,
                    d.DataResposta,
                    d.Respondido,
                    Questionario = d.Questionario.Titulo
                })
                .ToListAsync();

            return Ok(disparos);
        }
    }
}