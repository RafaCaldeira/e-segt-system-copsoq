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

        [HttpPost("{questionarioId}/dimensao/{dimensaoId}/pergunta")]
        public async Task<IActionResult> CreatePergunta(int questionarioId, int dimensaoId, [FromBody] PerguntaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Verificar se a Dimensão pai (e o Questionário) existem
            var dimensao = await _context.Dimensoes
                .FirstOrDefaultAsync(d => d.ID == dimensaoId && d.QuestionarioID == questionarioId);

            if (dimensao == null)
            {
                return NotFound($"Dimensão com ID {dimensaoId} (para Questionário {questionarioId}) não encontrada.");
            }

            // 2. Criar a nova Pergunta
            var novaPergunta = new Pergunta
            {
                Texto = dto.Texto,
                Tipo = dto.Tipo,
                QuestionarioID = questionarioId, // Liga ao Questionário
                DimensaoID = dimensaoId          // Liga à Dimensão
            };

            _context.Perguntas.Add(novaPergunta);
            await _context.SaveChangesAsync();

            // Retorna a pergunta criada
            // (Não precisamos de um 'GetPergunta' separado por agora)
            return Created($"api/questionario/{questionarioId}/dimensao/{dimensaoId}/pergunta/{novaPergunta.ID}", novaPergunta);
        }

        [HttpPost("{questionarioId}/dimensao")]
        public async Task<IActionResult> CreateDimensao(int questionarioId, [FromBody] DimensaoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Verificar se o Questionário pai existe
            var questionario = await _context.Questionarios.FindAsync(questionarioId);
            if (questionario == null)
            {
                return NotFound($"Questionário com ID {questionarioId} não encontrado.");
            }

            // 2. Criar a nova Dimensão
            var novaDimensao = new Dimensao
            {
                Titulo = dto.Titulo,
                NomeIndicador = dto.NomeIndicador,
                Ordem = dto.Ordem,
                QuestionarioID = questionarioId // Liga ao Questionário pai
            };

            _context.Dimensoes.Add(novaDimensao);
            await _context.SaveChangesAsync();

            // Retorna a dimensão criada
            return CreatedAtAction(nameof(GetDimensao), new { questionarioId = questionarioId, id = novaDimensao.ID }, novaDimensao);
            // (Vamos precisar criar o GetDimensao)
        }

        // GET: api/questionario/{questionarioId}/dimensao/{id} 
        // (Método auxiliar para o CreatedAtAction acima)
        [HttpGet("{questionarioId}/dimensao/{id}")]
        public async Task<IActionResult> GetDimensao(int questionarioId, int id)
        {
            var dimensao = await _context.Dimensoes
               .FirstOrDefaultAsync(d => d.ID == id && d.QuestionarioID == questionarioId);

            if (dimensao == null)
                return NotFound();

            return Ok(dimensao);
        }

    }
}