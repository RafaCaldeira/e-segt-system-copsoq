using System;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Planos
{
    public class AcaoStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Concluido", "Pendente", etc.

        public DateTime? DataConclusao { get; set; } // Pode ser nulo

        public string? Justificativa { get; set; } // Pode ser nulo
    }
}