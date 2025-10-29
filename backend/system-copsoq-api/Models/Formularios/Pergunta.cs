using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using system_copsoq_api.Models;

namespace system_copsoq_api.Models.Formularios
{
    public class Pergunta
    {
        public int ID { get; set; }

        [Required]
        public string Texto { get; set; } = string.Empty; // Ex: "O seu trabalho exige..."

        [Required]
        public TipoPergunta Tipo { get; set; } // Ex: Likert5Pontos

        // --- Chaves Estrangeiras ---

        // A que Questionário ela pertence
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!;


        public int DimensaoID { get; set; }
        public Dimensao Dimensao { get; set; } = null!;

        public ICollection<RespostaFuncionario> Respostas { get; set; } = new List<RespostaFuncionario>();
    }
}