using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Responder
{
    public class RespostaDto
    {
        [Required]
        public int PerguntaId { get; set; }

        // ADICIONE O '?' AQUI 👇
        [Range(0, 10, ErrorMessage = "O valor deve ser entre 0 e 10.")] 
        public int? ValorResposta { get; set; } 

        public string? TextoResposta { get; set; } 
    }
}