using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models;
using system_copsoq_api.Services; // <-- Importante para o IEmailService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

// Alias para evitar conflito de nomes
using DisparoModel = system_copsoq_api.Models.Disparo.Disparo;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DisparoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService; // <-- 1. Serviço Injetado

        // 2. Construtor Atualizado
        public DisparoController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/disparo
        [HttpPost]
        public async Task<IActionResult> CreateDisparos([FromBody] DisparoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validar Questionário
            var questionario = await _context.Questionarios.FindAsync(dto.QuestionarioID);
            if (questionario == null) return NotFound("Questionário não encontrado.");

            // 2. Validar Funcionários
            var funcionarios = await _context.Funcionarios
                .Where(f => dto.FuncionarioIDs.Contains(f.ID))
                .ToListAsync();

            if (!funcionarios.Any()) return BadRequest("Nenhum funcionário encontrado.");
            
            // Validação extra: verificar se todos pertencem à mesma empresa (opcional, mas recomendado)
            if (funcionarios.Select(f => f.EmpresaID).Distinct().Count() > 1)
            {
                 return BadRequest("Selecione funcionários de uma única empresa por vez.");
            }

            var novosDisparos = new List<DisparoModel>();
            var emailsEnviados = 0;

            foreach (var funcionario in funcionarios)
            {
                // Verifica se já existe um disparo pendente para evitar spam
                bool jaExiste = await _context.Disparos
                    .AnyAsync(d => d.QuestionarioID == dto.QuestionarioID && 
                                   d.FuncionarioID == funcionario.ID && 
                                   !d.Respondido);
                
                if (jaExiste) continue;

                // Cria o novo disparo
                var novoDisparo = new DisparoModel
                {
                    QuestionarioID = dto.QuestionarioID,
                    FuncionarioID = funcionario.ID,
                    DataEnvio = DateTime.UtcNow,
                    TokenAcesso = Guid.NewGuid(), // O Token Único
                    Respondido = false
                };
                novosDisparos.Add(novoDisparo);

                // 3. ENVIAR O E-MAIL
                try 
                {
                    // Link para o Front-end (Ajuste a porta se o seu Vue estiver noutra, ex: 5173)
                    string link = $"http://localhost:5173/responder/{novoDisparo.TokenAcesso}";

                    string assunto = $"Convite para Avaliação: {questionario.Titulo}";
                    
                    string corpoEmail = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2>Olá, {funcionario.Nome}!</h2>
                            <p>A sua empresa convidou-o a participar na avaliação: <strong>{questionario.Titulo}</strong>.</p>
                            <p>As suas respostas são confidenciais e ajudarão a melhorar o ambiente de trabalho.</p>
                            <br>
                            <a href='{link}' style='background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>ACESSAR QUESTIONÁRIO</a>
                            <br><br>
                            <p style='font-size: 0.9em; color: #666;'>Se o botão não funcionar, copie e cole este link no seu navegador:</p>
                            <p style='font-size: 0.9em; color: #666;'>{link}</p>
                        </div>
                    ";

                    // Envia o email (await aqui para garantir que sai)
                    await _emailService.SendEmailAsync(funcionario.Email, assunto, corpoEmail);
                    emailsEnviados++;
                }
                catch (Exception ex)
                {
                    // Se falhar o envio, registamos o erro mas continuamos (o disparo é salvo no banco na mesma)
                    Console.WriteLine($"[ERRO EMAIL] Falha ao enviar para {funcionario.Email}: {ex.Message}");
                }
            }

            if (!novosDisparos.Any()) return Ok("Nenhum novo disparo necessário (todos já têm envios pendentes).");

            // 4. Salvar no Banco
            _context.Disparos.AddRange(novosDisparos);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{novosDisparos.Count} disparos criados e {emailsEnviados} e-mails enviados com sucesso." });
        }

        // GET: api/disparo/historico
        [HttpGet("historico")]
        public async Task<ActionResult<IEnumerable<DisparoHistoricoDto>>> GetHistorico()
        {
             var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return Unauthorized();

            var query = _context.Disparos
                .Include(d => d.Funcionario)
                .Include(d => d.Questionario)
                .AsQueryable();

            if (user.Role == Role.Cliente)
            {
                query = query.Where(d => d.Funcionario.EmpresaID == user.EmpresaID);
            }

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
                    DataResposta = d.DataResposta,
                    Link = d.TokenAcesso.ToString() 
                })
                .ToListAsync();

            return Ok(historico);
        }
    }
}