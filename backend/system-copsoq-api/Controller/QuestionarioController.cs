using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models.Formularios; 
using system_copsoq_api.Models; 
using System.Linq;
using System.Threading.Tasks;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Só 'Administrador' pode acessar qualquer método deste controller
    //[Authorize(Roles = "Admin")] 
    public class QuestionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionarioController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/questionario
        [HttpPost]
        public async Task<IActionResult> CreateQuestionario([FromBody] QuestionarioCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Criar o Questionário mestre
            var novoQuestionario = new Questionario
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                TextoIntroducao = dto.TextoIntroducao,
                TextoConsentimento = dto.TextoConsentimento
            };

            // 2. Adicionar as "Etiquetas" de Setor (se houver)
            if (dto.SetoresAplicaveis.Any())
            {
                foreach (var setor in dto.SetoresAplicaveis)
                {
                    novoQuestionario.SetoresAplicaveis.Add(new QuestionarioSetorAplicavel
                    {
                        Setor = setor
                    });
                }
            }

            _context.Questionarios.Add(novoQuestionario);
            await _context.SaveChangesAsync();

            // Retorna o objeto completo (com o ID gerado)
            return CreatedAtAction(nameof(GetQuestionario), new { id = novoQuestionario.ID }, novoQuestionario);
        }
        
        // GET: api/questionario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionario(int id)
        {
            // Busca o questionário e inclui as 'Etiquetas' de setor
            var questionario = await _context.Questionarios
                .Include(q => q.SetoresAplicaveis) 
                .FirstOrDefaultAsync(q => q.ID == id);

            if (questionario == null)
                return NotFound();

            return Ok(questionario);
        }
    }
}