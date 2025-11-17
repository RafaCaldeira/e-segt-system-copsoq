using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Dashboard
{
    // DTO para o Admin cadastrar um "Tópico" ou "Indicador"
    public class DimensaoCreateDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty; // Ex: "Demandas e Exigências do Trabalho"

        [Required]
        public string NomeIndicador { get; set; } = string.Empty; // Ex: "Exigências cognitivas" (do seu relatório)

        [Required]
        public int Ordem { get; set; } // Ex: 1 (para a Página 1/6)
    }
}