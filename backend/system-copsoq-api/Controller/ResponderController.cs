using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs.Responder;
using system_copsoq_api.Models; // <-- Importar os DTOs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // GET: api/responder/{token}
        [HttpPost("{token}")]
        public async Task<IActionResult> SubmitRespostas(Guid token, [FromBody] SubmissaoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Encontrar o Disparo pelo Token
            var disparo = await _context.Disparos
                .Include(d => d.Questionario) 
                    .ThenInclude(q => q.Dimensoes) 
                        .ThenInclude(dim => dim.Perguntas) 
                .Include(d => d.Funcionario) 
                    .ThenInclude(f => f.Empresa) 
                .FirstOrDefaultAsync(d => d.TokenAcesso == token);

            // 2. Validar o Disparo
            if (disparo == null)
            {
                return NotFound("Link inválido ou expirado.");
            }
            if (disparo.Respondido)
            {
                return BadRequest("Este questionário já foi respondido.");
            }
            if (disparo.Funcionario == null)
            {
                return NotFound("Funcionário associado a este link não foi encontrado.");
            }

            // 3. Validar as Respostas Recebidas
            var idsPerguntasDoQuestionario = disparo.Questionario.Perguntas.Select(p => p.ID).ToList();
            var idsPerguntasRespondidas = dto.Respostas.Select(r => r.PerguntaId).ToList();

            // Verifica se todas as perguntas respondidas pertencem a este questionário
            if (idsPerguntasRespondidas.Except(idsPerguntasDoQuestionario).Any())
            {
                return BadRequest("Uma ou mais respostas são para perguntas que não pertencem a este questionário.");
            }
            
            disparo.Funcionario.CPF = dto.Cpf;

            // (Opcional: Verificar se TODAS as perguntas foram respondidas)
            if (idsPerguntasDoQuestionario.Except(idsPerguntasRespondidas).Any())
            {
                 //return BadRequest("Faltam respostas para algumas perguntas.");
                 // Ou podemos permitir respostas parciais, dependendo da regra de negócio
            }

            // 4. Salvar as Respostas no Banco
            var novasRespostas = dto.Respostas.Select(dto => new RespostaFuncionario
            {
                DisparoID = disparo.ID,
                PerguntaID = dto.PerguntaId,
                ValorResposta = dto.ValorResposta
            }).ToList();

            _context.RespostasFuncionarios.AddRange(novasRespostas);

            // 5. Marcar o Disparo como Respondido
            disparo.Respondido = true;
            disparo.DataResposta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Respostas enviadas com sucesso!" });
        }


        [HttpGet("{token}")]
        public async Task<ActionResult<QuestionarioParaResponderDto>> GetQuestionarioParaResponder(Guid token)
        {
            // 1. Encontrar o Disparo pelo Token único
            var disparo = await _context.Disparos
                .Include(d => d.Questionario) // Inclui dados do Questionário...
                    .ThenInclude(q => q.Dimensoes) // ...e suas Dimensões...
                        .ThenInclude(dim => dim.Perguntas) // ...e suas Perguntas
                    .Include(d => d.Funcionario) 
                        .ThenInclude(f => f.Empresa)
                .FirstOrDefaultAsync(d => d.TokenAcesso == token);

            // 2. Validar o Disparo
            if (disparo == null)
            {
                return NotFound("Link inválido ou expirado.");
            }
            if (disparo.Respondido)
            {
                return BadRequest("Este questionário já foi respondido.");
            }
            if (disparo.Funcionario == null || disparo.Funcionario.Empresa == null)
            {
                 return NotFound("Os dados do funcionário ou da empresa não foram encontrados.");
            }

            // 3. Montar o DTO de Resposta (para não enviar dados extras)
            var questionarioDto = new QuestionarioParaResponderDto
            {
                Id = disparo.Questionario.ID,
                Titulo = disparo.Questionario.Titulo,
                TextoIntroducao = disparo.Questionario.TextoIntroducao,
                TextoConsentimento = disparo.Questionario.TextoConsentimento,
                Dimensoes = disparo.Questionario.Dimensoes
                    .OrderBy(d => d.Ordem) // Ordena as "páginas"
                    .Select(d => new DimensaoRespostaDto
                    {
                        Id = d.ID,
                        Titulo = d.Titulo,
                        Ordem = d.Ordem,
                        Perguntas = d.Perguntas
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
                }
            };

            return Ok(questionarioDto);
        }
    }
}