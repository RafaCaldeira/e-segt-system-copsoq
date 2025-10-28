using system_copsoq_api.Models; // Para o SetorAtuacao
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    public class QuestionarioCreateDto
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;
        
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public string TextoIntroducao { get; set; } = string.Empty;

        [Required]
        public string TextoConsentimento { get; set; } = string.Empty;

        public List<SetorAtuacao> SetoresAplicaveis { get; set; } = new List<SetorAtuacao>();
    }
}