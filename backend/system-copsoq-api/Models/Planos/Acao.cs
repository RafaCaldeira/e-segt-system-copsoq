using System;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.Models.Planos
{
    public class Acao
    {
        public int ID { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty; // Ex: "Rever horário de descanso"

        public DateTime? Prazo { get; set; } // Data limite para concluir

        [Required]
        public StatusAcao Status { get; set; } = StatusAcao.Pendente;

        // --- Relação com o Plano ---
        [Required]
        public int PlanoDeAcaoID { get; set; }
        public PlanoDeAcao PlanoDeAcao { get; set; } = null!;
    }
}