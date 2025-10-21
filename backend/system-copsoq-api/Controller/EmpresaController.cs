using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Data;
using system_copsoq_api.Models;
using Microsoft.AspNetCore.Identity;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IPasswordHasher<Empresa> _passwordHasher;

        public EmpresaController(AppDbContext context, IPasswordHasher<Empresa> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/empresa
        [HttpGet]
        public IActionResult GetEmpresas()
        {
            var empresas = _context.Empresas.ToList();
            return Ok(empresas);
        }

        // GET: api/empresa/{id}
        [HttpGet("{id}")]
        public IActionResult GetEmpresa(int id)
        {
            var empresa = _context.Empresas.Find(id);
            if (empresa == null)
                return NotFound();
            return Ok(empresa);
        }

        // POST: api/empresa
        [HttpPost]
        public IActionResult CreateEmpresa([FromBody] Empresa empresa)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            empresa.Senha = _passwordHasher.HashPassword(empresa, empresa.Senha);    

            _context.Empresas.Add(empresa);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.ID }, empresa);
        }

        // PUT: api/empresa/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateEmpresa(int id, [FromBody] Empresa empresa)
        {
            var existing = _context.Empresas.Find(id);
            if (existing == null)
                return NotFound();

            existing.NomeEmpresa = empresa.NomeEmpresa;
            existing.NomeResponsavel = empresa.NomeResponsavel;
            existing.SetorAtuacao = empresa.SetorAtuacao;
            existing.Cidade = empresa.Cidade;
            existing.Email = empresa.Email;
            existing.Cnpj = empresa.Cnpj;

            if (!string.IsNullOrEmpty(empresa.Senha))
            {
                existing.Senha = _passwordHasher.HashPassword(existing, empresa.Senha);
            }

            _context.SaveChanges();
            return Ok(existing);
        }

        // DELETE: api/empresa/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteEmpresa(int id)
        {
            var empresa = _context.Empresas.Find(id);
            if (empresa == null)
                return NotFound();

            _context.Empresas.Remove(empresa);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
