using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs.Responder
{
    // Representa o "pacote" completo que o funcionário envia
    public class SubmissaoDto
    {
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        // (Pode adicionar validação de CPF [StringLength(11)] ou Regex aqui)
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Pelo menos uma resposta é necessária.")]
        public List<RespostaDto> Respostas { get; set; } = new List<RespostaDto>();
    }
}