using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs.Responder; 
using system_copsoq_api.DTOs;
using system_copsoq_api.Models.Disparo;
using system_copsoq_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Permite acesso sem login (crucial para o funcionário responder)
    public class ResponderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResponderController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // 1. GET: Puxar o questionário completo pelo Token
        // Rota: GET api/responder/{token}
        // ---------------------------------------------------------
        [HttpGet("{token}")]
        public async Task<ActionResult<QuestionarioParaResponderDto>> GetQuestionarioPeloToken(string token)
        {
            if (!Guid.TryParse(token, out var tokenGuid))
                return BadRequest("Token inválido.");

            // Busca o Disparo e carrega toda a árvore de dados necessária
            var disparo = await _context.Disparos
                .Include(d => d.Funcionario)
                    .ThenInclude(f => f.Empresa)
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.Dimensoes)
                        .ThenInclude(dim => dim.Perguntas) // Carrega perguntas dentro das dimensões
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.OpcoesResposta)
                .FirstOrDefaultAsync(d => d.TokenAcesso == tokenGuid);

            if (disparo == null)
                return NotFound("Questionário não encontrado ou link inválido.");

            if (disparo.Respondido)
                return BadRequest("Este questionário já foi respondido.");

            // Mapeamento Manual: Entidade -> DTO
            // (Assumindo que você tem os DTOs auxiliares: DimensaoRespostaDto, PerguntaRespostaDto, etc.)
            var dto = new QuestionarioParaResponderDto
            {
                Id = disparo.Questionario.ID,
                Titulo = disparo.Questionario.Titulo,
                TextoIntroducao = disparo.Questionario.TextoIntroducao,
                TextoConsentimento = disparo.Questionario.TextoConsentimento,
                
                // Mapeia Funcionário
                Funcionario = new FuncionarioSimplesDto
                {
                    Nome = disparo.Funcionario.Nome,
                    Cpf = disparo.Funcionario.CPF, 
                    Setor = disparo.Funcionario.Setor.ToString(),
                    NomeEmpresa = disparo.Funcionario.Empresa != null ? disparo.Funcionario.Empresa.NomeEmpresa : "Empresa"
                },

                // Mapeia Opções de Resposta
                OpcoesResposta = disparo.Questionario.OpcoesResposta
                    .OrderBy(o => o.Ordem)
                    .Select(o => new OpcaoRespostaDto 
                    {
                        // Ajuste os nomes das propriedades conforme seu OpcaoRespostaDto
                        Texto = o.Texto,
                        Valor = o.Valor,
                        Ordem = o.Ordem
                    }).ToList(),

                // Mapeia Dimensões e Perguntas (A parte mais importante)
                Dimensoes = disparo.Questionario.Dimensoes
                    .OrderBy(dim => dim.Ordem)
                    .Select(dim => new DimensaoRespostaDto
                    {
                        Id = dim.ID,
                        Titulo = dim.Titulo,
                        // Assumindo que seu DimensaoRespostaDto tem uma lista de PerguntaRespostaDto
                        Perguntas = dim.Perguntas.Select(p => new PerguntaRespostaDTO
                        {
                            Id = p.ID,
                            Texto = p.Texto
                        }).ToList()
                    }).ToList()
            };

            return Ok(dto);
        }

        // ---------------------------------------------------------
        // 2. POST: Receber e Salvar as Respostas
        // Rota: POST api/responder/{token}
        // ---------------------------------------------------------
        [HttpPost("{token}")]
        public async Task<IActionResult> EnviarRespostas(string token, [FromBody] SubmissaoDto submissao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Guid.TryParse(token, out var tokenGuid))
                return BadRequest("Token inválido.");

            // 1. Busca o Disparo para validar
            var disparo = await _context.Disparos
                .Include(d => d.Funcionario) // Precisamos do funcionário para checar o CPF se necessário
                .FirstOrDefaultAsync(d => d.TokenAcesso == tokenGuid);

            if (disparo == null) return NotFound("Disparo não encontrado.");
            
            if (disparo.Respondido) 
                return BadRequest("Você já respondeu este questionário.");

            // 2. Validação Opcional de CPF
            // Compara o CPF enviado no JSON com o CPF do funcionário no banco
            // Remove pontuação para evitar erros (ex: "123.456" vs "123456")
            /*
            var cpfBanco = disparo.Funcionario.CPF?.Replace(".", "").Replace("-", "").Trim();
            var cpfEnviado = submissao.Cpf?.Replace(".", "").Replace("-", "").Trim();

            if (cpfBanco != cpfEnviado)
            {
                 return BadRequest("O CPF informado não corresponde ao funcionário deste link.");
            }
            */

            // 3. Processar as Respostas
            var novasRespostas = new List<RespostaFuncionario>();

            foreach (var respDto in submissao.Respostas)
            {
                var novaResposta = new RespostaFuncionario
                {
                    DisparoID = disparo.ID,
                    PerguntaID = respDto.PerguntaId,
                    ValorResposta = respDto.ValorResposta // Mapeia ValorResposta do DTO para Valor da Model
                };
                novasRespostas.Add(novaResposta);
            }

            if (!novasRespostas.Any())
                return BadRequest("Nenhuma resposta foi enviada.");

            // 4. Salvar tudo no Banco
            _context.RespostasFuncionarios.AddRange(novasRespostas);

            // Atualiza status do Disparo
            disparo.Respondido = true;
            disparo.DataResposta = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Respostas salvas com sucesso! Obrigado pela participação." });
        }
    }
}