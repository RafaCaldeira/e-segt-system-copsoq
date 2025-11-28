using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs; // Para PerguntaCreateDto, OpcaoRespostaCreateDto
using system_copsoq_api.DTOs.Dashboard; // Para QuestionarioCreateDto
using system_copsoq_api.Models.Formularios; 
using system_copsoq_api.Models; 
using System.Linq;
using System.Threading.Tasks;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Reativando a segurança que tínhamos comentado
    [Authorize(Roles = "Admin")] // Use "Admin" (do seu enum Role.cs)
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

            var novoQuestionario = new Questionario
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                TextoIntroducao = dto.TextoIntroducao,
                TextoConsentimento = dto.TextoConsentimento
            };

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

            return CreatedAtAction(nameof(GetQuestionario), new { id = novoQuestionario.ID }, novoQuestionario);
        }

        // POST: api/questionario/{questionarioId}/opcao
        [HttpPost("{questionarioId}/opcao")]
        public async Task<IActionResult> CreateOpcaoResposta(int questionarioId, [FromBody] OpcaoRespostaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var questionario = await _context.Questionarios.FindAsync(questionarioId);
            if (questionario == null)
            {
                return NotFound($"Questionário com ID {questionarioId} não encontrado.");
            }

            var novaOpcao = new OpcaoResposta
            {
                Texto = dto.Texto,
                Valor = dto.Valor,
                Ordem = dto.Ordem,
                QuestionarioID = questionarioId
            };

            _context.OpcoesResposta.Add(novaOpcao);
            await _context.SaveChangesAsync();
            return Ok(novaOpcao);
        }

        // GET: api/questionario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionario(int id)
        {
            var questionario = await _context.Questionarios
                .Include(q => q.SetoresAplicaveis)
                .Include(q => q.Dimensoes)
                .Include(q => q.Perguntas)
                .Include(q => q.OpcoesResposta)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (questionario == null)
                return NotFound();

            return Ok(questionario);
        }

        // POST: api/questionario/{questionarioId}/dimensao/{dimensaoId}/pergunta
        [HttpPost("{questionarioId}/dimensao/{dimensaoId}/pergunta")]
        public async Task<IActionResult> CreatePergunta(int questionarioId, int dimensaoId, [FromBody] PerguntaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dimensao = await _context.Dimensoes
                .FirstOrDefaultAsync(d => d.ID == dimensaoId && d.QuestionarioID == questionarioId);

            if (dimensao == null)
            {
                return NotFound($"Dimensão com ID {dimensaoId} (para Questionário {questionarioId}) não encontrada.");
            }

            var novaPergunta = new Pergunta
            {
                Texto = dto.Texto,
                QuestionarioID = questionarioId,
                DimensaoID = dimensaoId 
            };

            _context.Perguntas.Add(novaPergunta);
            await _context.SaveChangesAsync();

            return Created($"api/questionario/{questionarioId}/dimensao/{dimensaoId}/pergunta/{novaPergunta.ID}", novaPergunta);
        }

        // POST: api/questionario/{questionarioId}/dimensao
        // (Esta é a versão CORRETA, que usa 'DimensaoCreateDto')
        [HttpPost("{questionarioId}/dimensao")]
        public async Task<IActionResult> CreateDimensao(int questionarioId, [FromBody] DimensaoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var questionario = await _context.Questionarios.FindAsync(questionarioId);
            if (questionario == null)
            {
                return NotFound($"Questionário com ID {questionarioId} não encontrado.");
            }

            var novaDimensao = new Dimensao
            {
                Titulo = dto.Titulo,
                NomeIndicador = dto.NomeIndicador,
                Ordem = dto.Ordem,
                QuestionarioID = questionarioId
            };

            _context.Dimensoes.Add(novaDimensao);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetDimensao), new { questionarioId = questionarioId, id = novaDimensao.ID }, novaDimensao);
        }

        // GET: api/questionario/{questionarioId}/dimensao/{id} 
        // (Este é o método auxiliar CORRETO)
        [HttpGet("{questionarioId}/dimensao/{id}")]
        public async Task<IActionResult> GetDimensao(int questionarioId, int id)
        {
             var dimensao = await _context.Dimensoes
                .FirstOrDefaultAsync(d => d.ID == id && d.QuestionarioID == questionarioId);

            if (dimensao == null)
                return NotFound();

            return Ok(dimensao);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestionarios()
        {
            var questionarios = await _context.Questionarios
                .Include(q => q.SetoresAplicaveis) // Importante: Incluir os setores!
                .ToListAsync();

            return Ok(questionarios);
        }
    }
}