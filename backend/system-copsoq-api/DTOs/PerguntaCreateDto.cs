using system_copsoq_api.Models.Formularios;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    public class PerguntaCreateDto
    {
        [Required]
        public string Texto { get; set; } = string.Empty; // Ex: "Com que frequência você se sente cansado(a) ao acordar?"

        [Required]
        public TipoPergunta Tipo { get; set; } = TipoPergunta.EscalaLikert;
    }
}