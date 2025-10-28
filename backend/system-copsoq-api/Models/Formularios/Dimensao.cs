using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.Models.Formularios
{
    // Esta é a "Dimensão", "Tópico" da página, ou "Indicador" do relatório
    public class Dimensao
    {
        public int ID { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty; // Ex: "Demandas e Exigências do Trabalho"
        
        [Required]
        public string NomeIndicador { get; set; } = string.Empty; // Ex: "Exigências cognitivas"

        public int Ordem { get; set; } // Para saber que esta é a página "1/6"

        // Chave Estrangeira para o Questionário
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!;

        // Relação (uma Dimensão agrupa várias perguntas)
        public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
    }
}