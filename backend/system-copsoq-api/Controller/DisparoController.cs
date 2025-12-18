using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models;
using system_copsoq_api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

// Alias para evitar conflito de nomes entre Model e Namespace
using DisparoModel = system_copsoq_api.Models.Disparo.Disparo;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Psicologo")] // Permite que logados acessem (Admin, Psicologo, Cliente)
    public class DisparoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public DisparoController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/disparo
        [HttpPost]
        [Authorize(Roles = "Admin, Psicologo")] // Apenas Admin e Psicologo podem disparar
        public async Task<IActionResult> CreateDisparos([FromBody] DisparoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validações
            var questionario = await _context.Questionarios.FindAsync(dto.QuestionarioID);
            if (questionario == null) return NotFound("Questionário não encontrado.");

            var funcionarios = await _context.Funcionarios
                .Where(f => dto.FuncionarioIDs.Contains(f.ID))
                .ToListAsync();

            if (!funcionarios.Any()) return BadRequest("Nenhum funcionário encontrado.");

            // 2. Preparar Lista
            var novosDisparos = new List<DisparoModel>();

            foreach (var funcionario in funcionarios)
            {
                // Evita duplicidade de envio pendente
                bool jaExiste = await _context.Disparos
                    .AnyAsync(d => d.QuestionarioID == dto.QuestionarioID && 
                                   d.FuncionarioID == funcionario.ID && 
                                   !d.Respondido);
                
                if (jaExiste) continue;

                var novoDisparo = new DisparoModel
                {
                    QuestionarioID = dto.QuestionarioID,
                    FuncionarioID = funcionario.ID,
                    DataEnvio = DateTime.UtcNow,
                    TokenAcesso = Guid.NewGuid(),
                    Respondido = false
                };
                
                novosDisparos.Add(novoDisparo);
            }

            if (!novosDisparos.Any()) 
                return Ok(new { Message = "Nenhum novo disparo criado. Todos já possuem pendências." });

            // 3. Salvar no Banco
            _context.Disparos.AddRange(novosDisparos);
            await _context.SaveChangesAsync();

            // 4. Enviar E-mails
            int emailsEnviados = 0;
            var errosEmail = new List<string>();

            foreach (var disparo in novosDisparos)
            {
                var funcionario = funcionarios.First(f => f.ID == disparo.FuncionarioID);

                try 
                {
                    // Ajuste a porta se necessário (ex: 5173 para Vite/Vue)
                    string link = $"http://localhost:5173/responder/{disparo.TokenAcesso}";
                    string assunto = $"Convite: {questionario.Titulo}";
                    string corpoEmail = $@"
                        <h2>Olá, {funcionario.Nome}</h2>
                        <p>Você tem um novo formulário disponível.</p>
                        <a href='{link}'>Clique aqui para responder</a>";

                    await _emailService.SendEmailAsync(funcionario.Email, assunto, corpoEmail);
                    emailsEnviados++;
                }
                catch (Exception ex)
                {
                    errosEmail.Add($"Erro ao enviar para {funcionario.Email}: {ex.Message}");
                }
            }

            return Ok(new 
            { 
                Message = $"Processo concluído. {novosDisparos.Count} formulários gerados.",
                EmailsSucesso = emailsEnviados,
                Erros = errosEmail
            });
        }

        // GET: api/disparo/historico
        [HttpGet("historico")]
        public async Task<ActionResult<IEnumerable<DisparoHistoricoDto>>> GetHistorico()
        {
            try 
            {
                // Identifica quem está logado
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);
                
                // Se não achar usuário ou role, bloqueia (segurança extra)
                if (user == null) return Unauthorized();

                // Monta a Query Base
                var query = _context.Disparos
                    .Include(d => d.Funcionario)
                    .Include(d => d.Questionario)
                    .AsQueryable();

                // SE FOR CLIENTE: Filtra apenas a empresa dele
                if (user.Role == Role.Cliente)
                {
                    query = query.Where(d => d.Funcionario.EmpresaID == user.EmpresaID);
                }

                // Projeta o resultado (Aqui incluímos o Setor e EmpresaId)
                var historico = await query
                    .OrderByDescending(d => d.DataEnvio)
                    .Select(d => new DisparoHistoricoDto
                    {
                        Id = d.ID,
                        NomeFuncionario = d.Funcionario.Nome,
                        EmailFuncionario = d.Funcionario.Email,
                        TituloQuestionario = d.Questionario.Titulo,
                        DataEnvio = d.DataEnvio,
                        Respondido = d.Respondido,
                        // DataResposta = d.DataResposta, // Descomente se tiver no DTO
                        Link = d.TokenAcesso.ToString(), // CORREÇÃO: É TokenAcesso, não Token

                        // NOVOS CAMPOS PARA O GRÁFICO:
                        Setor = d.Funcionario.Setor,
                        EmpresaId = d.Funcionario.EmpresaID
                    })
                    .ToListAsync();

                return Ok(historico);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [Authorize(Roles = "Psicologo, Admin")]
        [HttpGet("funcionario/{funcionarioId}/disparos")]
        public async Task<IActionResult> GetDisparosDoFuncionario(int funcionarioId)
        {
            var disparos = await _context.Disparos
                .Where(d => d.FuncionarioID == funcionarioId)
                .Select(d => new {
                    d.ID,
                    d.DataEnvio,
                    d.DataResposta,
                    d.Respondido,
                    Questionario = d.Questionario.Titulo
                })
                .ToListAsync();

            return Ok(disparos);
        }
    }
}