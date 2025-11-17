using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs.Responder;
using system_copsoq_api.Models; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using system_copsoq_api.Models.Disparo; // <-- Importar Disparo
using system_copsoq_api.Models.Formularios; // <-- Importar Formularios

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResponderController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/responder/{token}
        [HttpPost("{token}")]
        public async Task<IActionResult> SubmitRespostas(Guid token, [FromBody] SubmissaoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Encontrar o Disparo (Incluindo o Funcionário)
            var disparo = await _context.Disparos
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.Perguntas) // Só precisamos das Perguntas
                .Include(d => d.Funcionario) // Incluir o Funcionário para salvar o CPF
                .FirstOrDefaultAsync(d => d.TokenAcesso == token);

            // 2. Validar o Disparo
            if (disparo == null)
                return NotFound("Link inválido ou expirado.");
            if (disparo.Respondido)
                return BadRequest("Este questionário já foi respondido.");
            if (disparo.Funcionario == null)
                return NotFound("Funcionário associado a este link não foi encontrado.");

            // 3. Validar as Respostas Recebidas
            var idsPerguntasDoQuestionario = disparo.Questionario.Perguntas.Select(p => p.ID).ToList();
            var idsPerguntasRespondidas = dto.Respostas.Select(r => r.PerguntaId).ToList();

            if (idsPerguntasRespondidas.Except(idsPerguntasDoQuestionario).Any())
            {
                return BadRequest("Uma ou mais respostas são para perguntas que não pertencem a este questionário.");
            }
            
            disparo.Funcionario.CPF = dto.Cpf; // Salva o CPF

            // 4. Salvar as Respostas no Banco
            var novasRespostas = dto.Respostas.Select(r => new RespostaFuncionario
            {
                DisparoID = disparo.ID,
                PerguntaID = r.PerguntaId,
                ValorResposta = r.ValorResposta
            }).ToList();

            // O seu código aqui (AddRange) estava correto!
            _context.RespostasFuncionarios.AddRange(novasRespostas); 

            // 5. Marcar o Disparo como Respondido
            disparo.Respondido = true;
            disparo.DataResposta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Respostas enviadas com sucesso!" });
        }


        // GET: api/responder/{token}
        [HttpGet("{token}")]
        public async Task<ActionResult<QuestionarioParaResponderDto>> GetQuestionarioParaResponder(Guid token)
        {
            // 1. CORREÇÃO DA CONSULTA: Adicionado o '.' em falta
            var disparo = await _context.Disparos
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.Dimensoes)
                        .ThenInclude(dim => dim.Perguntas)
                .Include(d => d.Questionario) // (Este Include é separado)
                    .ThenInclude(q => q.OpcoesResposta) // <-- Carrega as Opções
                .Include(d => d.Funcionario) 
                    .ThenInclude(f => f.Empresa)
                .FirstOrDefaultAsync(d => d.TokenAcesso == token);

            // 2. Validar o Disparo
            if (disparo == null)
                return NotFound("Link inválido ou expirado.");
            if (disparo.Respondido)
                return BadRequest("Este questionário já foi respondido.");
            if (disparo.Funcionario == null || disparo.Funcionario.Empresa == null)
                return NotFound("Os dados do funcionário ou da empresa não foram encontrados.");

            // 3. Montar o DTO de Resposta
            var questionarioDto = new QuestionarioParaResponderDto
            {
                Id = disparo.Questionario.ID,
                Titulo = disparo.Questionario.Titulo,
                TextoIntroducao = disparo.Questionario.TextoIntroducao,
                TextoConsentimento = disparo.Questionario.TextoConsentimento,

                Dimensoes = disparo.Questionario.Dimensoes
                    .OrderBy(d => d.Ordem) 
                    .Select(d => new DimensaoRespostaDto
                    {
                        Id = d.ID,
                        Titulo = d.Titulo,
                        Ordem = d.Ordem,
                        Perguntas = d.Perguntas
                            .OrderBy(p => p.ID)
                            .Select(p => new PerguntaRespostaDTO
                            {
                                Id = p.ID,
                                Texto = p.Texto
                            }).ToList()
                    }).ToList(),

                Funcionario = new FuncionarioSimplesDto
                {
                    Nome = disparo.Funcionario.Nome,
                    Setor = disparo.Funcionario.Setor,
                    CPF = disparo.Funcionario.CPF,
                    NomeEmpresa = disparo.Funcionario.Empresa.NomeEmpresa
                },

                OpcoesResposta = disparo.Questionario.OpcoesResposta
                    .OrderBy(o => o.Ordem)
                    .Select(o => new OpcaoRespostaDto
                    {
                        Texto = o.Texto,
                        Valor = o.Valor,
                        Ordem = o.Ordem
                    }).ToList()
            };

            return Ok(questionarioDto);
        }
    }
}