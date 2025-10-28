using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.Models.Formularios
{
    public class Questionario
    {
        public int ID { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty; // Ex: "Questionário COPSOQ"
        
        public string Descricao { get; set; } = string.Empty;

        // --- Textos do seu Esboço de Introdução ---
        [Required]
        public string TextoIntroducao { get; set; } = string.Empty; // O "Bem-vindo(a)..."
        
        [Required]
        public string TextoConsentimento { get; set; } = string.Empty; // O "Li e estou de acordo..."

        // Relações (Um Questionário tem...)
        public ICollection<Dimensao> Dimensoes { get; set; } = new List<Dimensao>();
        public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
        
        // Relação com os Setores (a "Etiqueta")
        public ICollection<QuestionarioSetorAplicavel> SetoresAplicaveis { get; set; } = new List<QuestionarioSetorAplicavel>();
    }
}