using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


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
        [Authorize(Roles = "Admin")]
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

            // 5. Lógica de UPDATE LIMPA (sem senhas ou emails)
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
    }
}
