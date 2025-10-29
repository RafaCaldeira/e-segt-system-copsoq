using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Responder
{
    // Representa a resposta a UMA pergunta
    public class RespostaDto
    {
        [Required]
        public int PerguntaId { get; set; }

        [Required]
        [Range(0, 4, ErrorMessage = "O valor da resposta deve estar entre 0 e 4.")] // Validação para Likert 5 pontos
        public int ValorResposta { get; set; } 
        // Ex: Sempre=4, Frequentemente=3, Às vezes=2, Raramente=1, Nunca=0
    }
}