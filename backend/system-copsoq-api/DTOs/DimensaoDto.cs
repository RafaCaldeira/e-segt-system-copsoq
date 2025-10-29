using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    public class DimensaoDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty; // Ex: "Demandas e Exigências..."

        [Required]
        public string NomeIndicador { get; set; } = string.Empty; // Ex: "Exigências cognitivas"

        [Required]
        public int Ordem { get; set; } // Ex: 1, 2, 3...
    }
}