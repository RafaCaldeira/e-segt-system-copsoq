using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs.Dashboard; // O seu namespace de DTOs
using system_copsoq_api.Models; // Para Role, User
using system_copsoq_api.Models.Disparo; // Para Disparo, RespostaFuncionario
using system_copsoq_api.Models.Formularios; // Para Questionario, Pergunta
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Security.Claims; // Para ler o ID do utilizador

namespace system_copsoq_api.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Proteger o controller inteiro
    public class RelatorioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatorioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/relatorio/empresa/1/questionario/6
        [HttpGet("empresa/{empresaId}/questionario/{questionarioId}")]
        [Authorize(Roles = "Admin,Cliente,Psicologo")] // Quem pode ver relatórios
        public async Task<ActionResult<RelatorioCompletoDto>> GetRelatorioEmpresa(int empresaId, int questionarioId)
        {
            // --- 1. Validação de Segurança ---
            // Um 'Cliente' só pode ver os relatórios da sua própria empresa.
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return Unauthorized();

            // Verifique o nome da sua Role (Admin ou Administrador)
            if (user.Role == Role.Cliente && user.EmpresaID != empresaId)
            {
                return Forbid("Acesso negado. Você só pode ver relatórios da sua própria empresa.");
            }

            // --- 2. Buscar Dados Base ---
            var questionario = await _context.Questionarios
                .Include(q => q.Dimensoes) // Precisamos das Dimensões/Indicadores
                .FirstOrDefaultAsync(q => q.ID == questionarioId);

            if (questionario == null)
            {
                return NotFound("Questionário não encontrado.");
            }

            // --- 3. Buscar as Respostas (O Coração do Cálculo) ---
            // Buscar todos os IDs de disparos respondidos daquela empresa e questionário
            var disparosRespondidosIds = await _context.Disparos
                .Where(d => d.Funcionario.EmpresaID == empresaId &&
                            d.QuestionarioID == questionarioId &&
                            d.Respondido)
                .Select(d => d.ID) // Só precisamos dos IDs dos disparos
                .ToListAsync();

            if (!disparosRespondidosIds.Any())
            {
                // Ainda não há respostas suficientes para gerar um relatório
                return NotFound("Nenhum questionário respondido encontrado para esta empresa.");
            }

            // Buscar todas as respostas associadas a esses disparos
            var respostas = await _context.RespostasFuncionarios
                .Where(r => disparosRespondidosIds.Contains(r.DisparoID))
                .Include(r => r.Pergunta) // Precisamos de saber a que Pergunta/Dimensão a resposta pertence
                .ToListAsync();

            if (!respostas.Any())
            {
                return NotFound("Nenhuma resposta individual encontrada.");
            }

            var empresa = await _context.Empresas.FindAsync(empresaId);

            // --- 4. O "Motor" de Cálculo ---
            var relatorio = new RelatorioCompletoDto
            {
                NomeEmpresa = empresa != null ? empresa.NomeEmpresa : "Empresa Desconhecida", // <--- Preencher aqui
                TituloQuestionario = questionario.Titulo,
                TotalRespondentes = disparosRespondidosIds.Count,
                DataGeracaoRelatorio = DateTime.UtcNow,
                Resultados = new List<ResultadoIndicadorDto>()
            };

            // Agrupar as respostas por Dimensão (Indicador)
            var respostasPorDimensao = respostas.GroupBy(r => r.Pergunta.DimensaoID);

            foreach (var grupo in respostasPorDimensao)
            {
                var dimensao = questionario.Dimensoes.FirstOrDefault(d => d.ID == grupo.Key);
                if (dimensao == null) continue; // Ignora se a dimensão não for encontrada

                // Calcular a Média (como o seu PDF de Apoio Social pede [cite: 36-39])
                var media = grupo.Average(r => r.ValorResposta);

                // Aplicar as Regras de Risco
                // TODO: Tornar esta função dinâmica para o Burnout (0-6) [cite: 43-45]
                string nivelRisco = CalcularNivelRiscoApoioSocial(media); // Função auxiliar

                relatorio.Resultados.Add(new ResultadoIndicadorDto
                {
                    NomeIndicador = dimensao.NomeIndicador,
                    // Convertendo a média (1-5) para percentual (0-100)
                    // (Valor - Mín) / (Máx - Mín)
                    // (media - 1) / (5 - 1) * 100
                    ScorePercentual = Math.Round((media - 1) / 4 * 100, 1), // Ex: 3.8 -> 70.0
                    NivelRisco = nivelRisco
                });
            }

            return Ok(relatorio);
        }

        // --- 5. Função Auxiliar (Baseada no seu PDF de Apoio Social [cite: 36-39]) ---
        private string CalcularNivelRiscoApoioSocial(double media)
        {
            if (media <= 2.4) return "Baixo";
            if (media <= 3.4) return "Médio";
            if (media <= 5.0) return "Baixo"; // (No seu esboço, "Apoio Social" 88% é "Baixo Risco")
            return "Indefinido";
        }
    }
}