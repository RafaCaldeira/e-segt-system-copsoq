using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace system_copsoq_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DisparoController : ControllerBase
    {
       private readonly AppDbContext _context;

        public DisparoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDisparos([FromBody] DisparoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Validar se o Questionário existe
            var questionario = await _context.Questionarios.FindAsync(dto.QuestionarioID);
            if (questionario == null)
            {
                return NotFound($"Questionário com ID {dto.QuestionarioID} não encontrado.");
            }

            // 2. Validar se os Funcionários existem e pertencem à mesma empresa
            var funcionarios = await _context.Funcionarios
                .Where(f => dto.FuncionarioIDs.Contains(f.ID))
                .ToListAsync();

            if (funcionarios.Count != dto.FuncionarioIDs.Count)
            {
                return BadRequest("Um ou mais IDs de funcionário são inválidos.");
            }

            // (Opcional, mas recomendado: Verificar se todos os funcionários são da mesma empresa)
            if (funcionarios.Select(f => f.EmpresaID).Distinct().Count() > 1)
            {
                return BadRequest("Todos os funcionários selecionados devem pertencer à mesma empresa.");
            }

            // 3. Criar um Disparo para cada Funcionário
            var novosDisparos = new List<Disparo>();
            foreach (var funcionario in funcionarios)
            {
                // (Opcional: Verificar se já existe um disparo pendente para este funcionário/questionário)
                bool jaExistePendente = await _context.Disparos
                    .AnyAsync(d => d.QuestionarioID == dto.QuestionarioID &&
                                   d.FuncionarioID == funcionario.ID &&
                                   !d.Respondido);
                
                if (jaExistePendente) continue; // Pula se já foi enviado e não respondido

                var novoDisparo = new Disparo
                {
                    QuestionarioID = dto.QuestionarioID,
                    FuncionarioID = funcionario.ID,
                    DataEnvio = DateTime.UtcNow,
                    TokenAcesso = Guid.NewGuid(), // Link único
                    Respondido = false
                };
                novosDisparos.Add(novoDisparo);
            }

            if (!novosDisparos.Any())
            {
                return Ok("Nenhum novo disparo necessário (todos já enviados ou funcionários inválidos).");
            }

            _context.Disparos.AddRange(novosDisparos);
            await _context.SaveChangesAsync();

            // Retorna os disparos criados (ou apenas uma mensagem de sucesso)
            // return Ok(novosDisparos); // Retorna os objetos criados
            return Ok(new { Message = $"{novosDisparos.Count} questionário(s) disparado(s) com sucesso." });
        }
    }
}