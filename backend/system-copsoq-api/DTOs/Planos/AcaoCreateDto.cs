using System;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Planos
{
    public class AcaoCreateDto
    {
        [Required]
        public string Descricao { get; set; } = string.Empty; // Ex: "Reunião de Feedback"

        public DateTime? Prazo { get; set; } // Opcional
    }
}