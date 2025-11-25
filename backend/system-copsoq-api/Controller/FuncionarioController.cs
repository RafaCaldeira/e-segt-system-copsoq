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
// NOVOS IMPORTS PARA CSV
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

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


        [HttpPost("importar")]
        [Authorize(Roles = "Cliente")] // Só Clientes importam os seus próprios funcionários
        public async Task<IActionResult> ImportarCsv(IFormFile file)
        {
            // 1. Validações Básicas
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum ficheiro enviado.");

            if (!file.FileName.EndsWith(".csv"))
                return BadRequest("O ficheiro deve ser um CSV.");

            // 2. Identificar a Empresa do Cliente
            var user = await GetUserFromToken();
            if (user == null || user.EmpresaID == null)
                return Forbid("Utilizador inválido.");

            var funcionariosCriados = 0;
            var erros = new List<string>();

            try
            {
                // 3. Ler o CSV
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";", // Assumindo que o CSV usa ponto e vírgula (comum no Excel/Brasil)
                    HasHeaderRecord = true,
                    MissingFieldFound = null // Ignora colunas em falta (opcional)
                }))
                {
                    // Lê os registos para o DTO
                    var registros = csv.GetRecords<FuncionarioCsvDto>().ToList();

                    foreach (var item in registros)
                    {
                        // (Opcional) Validações extra por linha
                        if (string.IsNullOrWhiteSpace(item.Nome) || string.IsNullOrWhiteSpace(item.Email))
                        {
                            erros.Add($"Linha ignorada (Nome ou Email em falta): {item.Nome}");
                            continue;
                        }

                        // Verifica se já existe este email na empresa (evita duplicados)
                        bool jaExiste = await _context.Funcionarios.AnyAsync(f => f.Email == item.Email && f.EmpresaID == user.EmpresaID);
                        if (jaExiste)
                        {
                            erros.Add($"Email já registado: {item.Email}");
                            continue;
                        }

                        var novoFunc = new Funcionario
                        {
                            Nome = item.Nome,
                            Email = item.Email,
                            Telefone = item.Telefone,
                            Cargo = item.Cargo,
                            Setor = item.Setor,
                            CPF = item.CPF,
                            EmpresaID = user.EmpresaID.Value
                        };

                        _context.Funcionarios.Add(novoFunc);
                        funcionariosCriados++;
                    }
                    
                    // Salva tudo de uma vez no final
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao processar o ficheiro: {ex.Message}. Verifique se o formato está correto (separador ';').");
            }

            return Ok(new 
            { 
                Message = $"{funcionariosCriados} funcionários importados com sucesso.", 
                Erros = erros 
            });
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