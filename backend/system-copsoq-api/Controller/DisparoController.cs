using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs;
using system_copsoq_api.Models; // Para Role
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. A CORREÇÃO: Usar um "apelido" (alias)
// Damos o apelido 'DisparoModel' para a classe 'Disparo' que está no namespace '...Models.Disparo'
using DisparoModel = system_copsoq_api.Models.Disparo.Disparo;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // <-- Use "Admin" (do seu enum Role.cs)
    public class DisparoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DisparoController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/disparo
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

            // 2. Validar se os Funcionários existem
            var funcionarios = await _context.Funcionarios
                .Where(f => dto.FuncionarioIDs.Contains(f.ID))
                .ToListAsync();

            if (funcionarios.Count != dto.FuncionarioIDs.Count)
            {
                return BadRequest("Um ou mais IDs de funcionário são inválidos.");
            }
            if (funcionarios.Select(f => f.EmpresaID).Distinct().Count() > 1)
            {
                return BadRequest("Todos os funcionários selecionados devem pertencer à mesma empresa.");
            }

            // 3. Criar os Disparos
            // 2. CORREÇÃO: Usar o apelido na 'List<>'
            var novosDisparos = new List<DisparoModel>(); 
            foreach (var funcionario in funcionarios)
            {
                bool jaExistePendente = await _context.Disparos
                    .AnyAsync(d => d.QuestionarioID == dto.QuestionarioID &&
                                   d.FuncionarioID == funcionario.ID &&
                                   !d.Respondido);
                
                if (jaExistePendente) continue; 

                // 3. CORREÇÃO: Usar o apelido no 'new'
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
            {
                return Ok("Nenhum novo disparo necessário (todos já enviados ou funcionários inválidos).");
            }

            // Esta linha agora funciona (corrige o CS1503)
            _context.Disparos.AddRange(novosDisparos);
            await _context.SaveChangesAsync();
            
            return Ok(new { Message = $"{novosDisparos.Count} questionário(s) disparado(s) com sucesso." });
        }
    }
}