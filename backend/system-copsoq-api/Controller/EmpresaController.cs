using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using system_copsoq_api.DTOs;
using system_copsoq_api.DTOs.Dashboard;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpresaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpresaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/empresa
        // --- CORREÇÃO AQUI ---
        // Adicionamos "Psicologo" para ele poder ver a lista de empresas no Dashboard/Relatórios
        [Authorize(Roles = "Admin,Psicologo")] 
        [HttpGet]
        public async Task<IActionResult> GetEmpresas()
        {
            var empresas = await _context.Empresas
                .Where(e => e.IsAtivo) 
                .ToListAsync();
            
            return Ok(empresas);
        }

        // GET: api/empresa/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null || !empresa.IsAtivo) 
            {
                return NotFound();
            }
            return Ok(empresa);
        }

        // PUT: api/empresa/{id}
        [HttpPut("{id}")]
        // Nota: Se quiser que o psicólogo EDITE dados cadastrais da empresa, adicione ele aqui também.
        // Se for só para ver relatórios, deixe como está.
        [Authorize(Roles = "Administrador,Cliente")] 
        public async Task<IActionResult> UpdateEmpresa(int id, [FromBody] Empresa empresaUpdate)
        {
            if (id != empresaUpdate.ID)
            {
                return BadRequest("O ID da URL não corresponde ao ID do corpo.");
            }

            var existing = await _context.Empresas.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.NomeEmpresa = empresaUpdate.NomeEmpresa;
            existing.NomeResponsavel = empresaUpdate.NomeResponsavel;
            existing.SetorAtuacao = empresaUpdate.SetorAtuacao;
            existing.Cidade = empresaUpdate.Cidade;
            existing.Cnpj = empresaUpdate.Cnpj;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return Ok(existing);
        }

        // DELETE: api/empresa/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();

            empresa.IsAtivo = false;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // ... (Os outros métodos GetEmpresasParaPsicologo e GetListaFuncionarios já estavam corretos) ...
        
        // GET: api/empresa/para-psicologo
        [HttpGet("para-psicologo")]
        [Authorize(Roles = "Psicologo,Admin")]
        public async Task<IActionResult> GetEmpresasParaPsicologo()
        {
            var empresas = await _context.Empresas
                .Where(e => e.IsAtivo)
                .Select(e => new {
                    e.ID,
                    e.NomeEmpresa
                })
                .ToListAsync();

            return Ok(empresas);
        }

        [HttpGet("{empresaId}/lista-funcionarios")]
        [Authorize(Roles = "Admin, Psicologo")] 
        public async Task<IActionResult> GetListaFuncionarios(int empresaId)
        {
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.ID == empresaId);
            if (!empresaExiste) return NotFound("Empresa não encontrada.");

            var listaFuncionarios = await _context.Funcionarios
                .Where(f => f.EmpresaID == empresaId)
                .Select(f => new FuncionarioListaDto
                {
                    Id = f.ID,
                    Nome = f.Nome,
                    Cargo = f.Cargo, 
                    QuestionariosRespondidos = f.Disparos
                        .Where(d => d.Respostas.Any()) 
                        .Select(d => new QuestionarioResumoDto
                        {
                            Titulo = d.Questionario.Titulo,
                            DataResposta = d.DataResposta ?? d.DataEnvio, 
                            TokenAcesso = d.TokenAcesso.ToString()
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(listaFuncionarios);
        }
    }
}