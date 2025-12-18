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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Reativando a segurança que tínhamos comentado
    [Authorize(Roles = "Admin, Psicologo")] // Use "Admin" (do seu enum Role.cs)
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

        [HttpGet("respostas/{token}")]
        [Authorize(Roles = "Psicologo")]
        public async Task<IActionResult> GetRespostasDetalhadas(string token)
        {
            // 1. Tenta converter a string para Guid. Se falhar, retorna erro.
            if (!Guid.TryParse(token, out var tokenGuid))
            {
                return BadRequest("O token fornecido não é válido.");
            }

            // 2. Usa a variável convertida (tokenGuid) na busca
            // ATENÇÃO: Mudei para ToListAsync, pois um token geralmente tem VÁRIAS respostas
            var respostas = await _context.RespostasFuncionarios
                .Include(r => r.Pergunta)
                .Include(r => r.Disparo)
                .Where(r => r.Disparo.TokenAcesso == tokenGuid) // 'Where' para pegar todas
                .ToListAsync();

            if (respostas == null || !respostas.Any())
                return NotFound("Nenhuma resposta encontrada para este token.");

            return Ok(respostas);
        }

        [HttpGet("download-pdf/{token}")]
        [AllowAnonymous] // <--- IMPORTANTE: Permite baixar clicando no link sem precisar de login
        public async Task<IActionResult> DownloadRelatorioIndividual(string token)
        {
            // 1. Validar Token
            if (!Guid.TryParse(token, out var tokenGuid)) 
                return BadRequest("Token inválido");

            // 2. Buscar Dados (Disparo + Respostas + Perguntas)
            var dados = await _context.Disparos
                .Include(d => d.Funcionario)
                .Include(d => d.Questionario)
                .Include(d => d.Respostas)
                    .ThenInclude(r => r.Pergunta) // Traz o texto da pergunta
                .FirstOrDefaultAsync(d => d.TokenAcesso == tokenGuid);

            if (dados == null) return NotFound("Questionário não encontrado.");
            if (!dados.Respostas.Any()) return BadRequest("Este questionário ainda não foi respondido.");

            // 3. Configuração do PDF
            QuestPDF.Settings.License = LicenseType.Community;

            // 4. Desenhar o PDF
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(2, Unit.Centimetre);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    // -- Cabeçalho --
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Relatório Individual de Respostas").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Questionário: {dados.Questionario.Titulo}").FontSize(14).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // -- Conteúdo --
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Dados do Funcionário
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row => 
                        {
                            row.RelativeItem().Text($"Funcionário: {dados.Funcionario.Nome}");
                            row.RelativeItem().AlignRight().Text($"Respondido em: {dados.DataResposta?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}");
                        });
                        
                        col.Item().PaddingTop(15);

                        // Tabela de Respostas
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Coluna da Pergunta (mais larga)
                                columns.RelativeColumn(1); // Coluna da Resposta
                            });

                            // Cabeçalho da Tabela
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Pergunta").SemiBold();
                                header.Cell().Element(CellStyle).Text("Nota").SemiBold();
                            });

                            // Linhas da Tabela
                            foreach (var resposta in dados.Respostas)
                            {
                                table.Cell().Element(CellStyle).Text(resposta.Pergunta.Texto);
                                
                                // Aqui mostramos o Valor (1 a 5). Se quiser converter para texto (ex: "Sempre"), precisaria fazer um switch/case ou join com Opcoes
                                table.Cell().Element(CellStyle).Text(resposta.ValorResposta.ToString()); 
                            }

                            // Estilo auxiliar para células
                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                            }
                        });
                    });

                    // -- Rodapé --
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            });

            // 5. Gerar o arquivo na memória e retornar
            var stream = new MemoryStream();
            documento.GeneratePdf(stream);
            stream.Position = 0;

            string nomeArquivo = $"Relatorio_{dados.Funcionario.Nome.Replace(" ", "_")}.pdf";
            return File(stream, "application/pdf", nomeArquivo);
        }
    }
}