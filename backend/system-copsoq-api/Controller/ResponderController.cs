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
    [AllowAnonymous] // Permite acesso sem login
    public class ResponderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResponderController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // 1. GET: Puxar o questionário completo pelo Token
        // ---------------------------------------------------------
        [HttpGet("{token}")]
        public async Task<ActionResult<QuestionarioParaResponderDto>> GetQuestionarioPeloToken(string token)
        {
            if (!Guid.TryParse(token, out var tokenGuid))
                return BadRequest("Token inválido.");

            var disparo = await _context.Disparos
                .Include(d => d.Funcionario)
                    .ThenInclude(f => f.Empresa)
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.Dimensoes)
                        .ThenInclude(dim => dim.Perguntas)
                .Include(d => d.Questionario)
                    .ThenInclude(q => q.OpcoesResposta)
                .FirstOrDefaultAsync(d => d.TokenAcesso == tokenGuid);

            if (disparo == null)
                return NotFound("Questionário não encontrado ou link inválido.");

            if (disparo.Respondido)
                return BadRequest("Este questionário já foi respondido.");

            // Mapeamento para DTO
            var dto = new QuestionarioParaResponderDto
            {
                Id = disparo.Questionario.ID,
                Titulo = disparo.Questionario.Titulo,
                TextoIntroducao = disparo.Questionario.TextoIntroducao,
                TextoConsentimento = disparo.Questionario.TextoConsentimento,
                
                Funcionario = new FuncionarioSimplesDto
                {
                    Nome = disparo.Funcionario.Nome,
                    Cpf = disparo.Funcionario.CPF, 
                    Setor = disparo.Funcionario.Setor.ToString(),
                    NomeEmpresa = disparo.Funcionario.Empresa?.NomeEmpresa ?? "Empresa"
                },

                OpcoesResposta = disparo.Questionario.OpcoesResposta
                    .OrderBy(o => o.Ordem)
                    .Select(o => new OpcaoRespostaDto 
                    {
                        Texto = o.Texto,
                        Valor = o.Valor,
                        Ordem = o.Ordem
                    }).ToList(),

                Dimensoes = disparo.Questionario.Dimensoes
                    .OrderBy(dim => dim.Ordem)
                    .Select(dim => new DimensaoRespostaDto
                    {
                        Id = dim.ID,
                        Titulo = dim.Titulo,
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
        // 2. POST: Receber e Salvar as Respostas (CORRIGIDO)
        // ---------------------------------------------------------
        [HttpPost("{token}")]
        public async Task<IActionResult> EnviarRespostas(string token, [FromBody] SubmissaoDto submissao)
        {
            // 1. Basic Model Validation
            if (!ModelState.IsValid)
            {
                var erros = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest($"Dados inválidos: {erros}");
            }

            if (!Guid.TryParse(token, out var tokenGuid))
                return BadRequest("Token inválido.");

            try 
            {
                var disparo = await _context.Disparos
                    .Include(d => d.Funcionario)
                    .FirstOrDefaultAsync(d => d.TokenAcesso == tokenGuid);

                if (disparo == null) return NotFound("Disparo não encontrado.");
                
                if (disparo.Respondido) 
                    return BadRequest("Você já respondeu este questionário.");

                // 2. Duplicate Protection (Crucial Fix)
                // Groups by QuestionId and takes the first answer to prevent DB constraint errors
                var respostasUnicas = submissao.Respostas
                                        .GroupBy(r => r.PerguntaId)
                                        .Select(g => g.First())
                                        .ToList();

                var novasRespostas = new List<RespostaFuncionario>();

                foreach (var respDto in respostasUnicas)
                {
                    var novaResposta = new RespostaFuncionario
                    {
                        DisparoID = disparo.ID,
                        PerguntaID = respDto.PerguntaId
                    };

                    // Logic to determine what to save
                    if (!string.IsNullOrEmpty(respDto.TextoResposta))
                    {
                        // It's a text answer
                        novaResposta.TextoResposta = respDto.TextoResposta;
                    }
                    else if (respDto.ValorResposta.HasValue)
                    {
                        // It's a numeric answer
                        novaResposta.ValorResposta = respDto.ValorResposta.Value;
                    }

                    // Optional: Skip if both are empty (unless you want to allow empty answers)
                    if (string.IsNullOrEmpty(novaResposta.TextoResposta) && novaResposta.ValorResposta == null)
                    {
                        continue; 
                    }

                    novasRespostas.Add(novaResposta);
                }

                if (!novasRespostas.Any())
                    return BadRequest("Nenhuma resposta válida foi enviada.");

                _context.RespostasFuncionarios.AddRange(novasRespostas);

                disparo.Respondido = true;
                disparo.DataResposta = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Respostas salvas com sucesso!" });
            }
            catch (DbUpdateException dbEx)
            {
                // returns the specific DB error (e.g., duplicate key)
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return BadRequest($"Erro ao salvar no banco de dados: {innerMessage}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno no servidor: {ex.Message}");
            }
        }
    }
}