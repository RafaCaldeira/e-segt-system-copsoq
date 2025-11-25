using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using system_copsoq_api.Data;
using system_copsoq_api.DTOs.Planos; // O seu novo namespace
using system_copsoq_api.Models.Planos; // O namespace dos Models
using system_copsoq_api.Models; // Para Role
using System.Linq;
using System.Threading.Tasks;
using System;

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegido
    public class PlanoDeAcaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlanoDeAcaoController(AppDbContext context)
        {
            _context = context;
        }

        // 1. CRIAR UM NOVO PLANO
        // POST: api/planodeacao
        [HttpPost]
        [Authorize(Roles = "Admin,Psicologo")] // Só Admin/Psicologo criam planos
        public async Task<IActionResult> CreatePlano([FromBody] PlanoDeAcaoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var novoPlano = new PlanoDeAcao
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                EmpresaID = dto.EmpresaID,
                DataCriacao = DateTime.UtcNow,
                IsAtivo = true
            };

            _context.PlanosDeAcao.Add(novoPlano);
            await _context.SaveChangesAsync();

            return Ok(novoPlano);
        }

        // 2. OBTER PLANOS DE UMA EMPRESA
        // GET: api/planodeacao/empresa/{empresaId}
        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> GetPlanosPorEmpresa(int empresaId)
        {
            // (Pode adicionar validação aqui para impedir que Cliente A veja planos da Empresa B)

            var planos = await _context.PlanosDeAcao
                .Where(p => p.EmpresaID == empresaId && p.IsAtivo)
                .Include(p => p.Acoes) // Traz as tarefas junto!
                .ToListAsync();

            return Ok(planos);
        }

        // 3. ADICIONAR UMA TAREFA (AÇÃO) A UM PLANO
        // POST: api/planodeacao/{planoId}/acao
        [HttpPost("{planoId}/acao")]
        [Authorize(Roles = "Admin,Psicologo")]
        public async Task<IActionResult> AddAcao(int planoId, [FromBody] AcaoCreateDto dto)
        {
            var plano = await _context.PlanosDeAcao.FindAsync(planoId);
            if (plano == null) return NotFound("Plano não encontrado");

            var novaAcao = new Acao
            {
                Descricao = dto.Descricao,
                Prazo = dto.Prazo,
                Status = StatusAcao.Pendente,
                PlanoDeAcaoID = planoId
            };

            _context.Acoes.Add(novaAcao);
            await _context.SaveChangesAsync();

            return Ok(novaAcao);
        }

        // 4. MUDAR STATUS DA TAREFA (Concluir)
        // PUT: api/planodeacao/acao/{acaoId}/concluir
        [HttpPut("acao/{acaoId}/concluir")]
        public async Task<IActionResult> ConcluirAcao(int acaoId)
        {
            var acao = await _context.Acoes.FindAsync(acaoId);
            if (acao == null) return NotFound("Ação não encontrada");

            acao.Status = StatusAcao.Concluido;
            await _context.SaveChangesAsync();

            return Ok(acao);
        }
    }
}