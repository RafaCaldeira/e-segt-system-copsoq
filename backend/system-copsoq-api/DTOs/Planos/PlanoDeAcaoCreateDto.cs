using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Planos
{
    public class PlanoDeAcaoCreateDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty; // Ex: "Melhoria do Ambiente"

        public string Descricao { get; set; } = string.Empty;

        [Required]
        public int EmpresaID { get; set; } // Para qual empresa é este plano?
    }
}