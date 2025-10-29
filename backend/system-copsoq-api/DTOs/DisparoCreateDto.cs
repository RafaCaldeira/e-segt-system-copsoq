using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace system_copsoq_api.DTOs
{
    public class DisparoCreateDto
    {
        [Required]
        public int QuestionarioID { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Selecione pelo menos um funcionário.")]
        public List<int> FuncionarioIDs { get; set; } = new List<int>();
    }
}