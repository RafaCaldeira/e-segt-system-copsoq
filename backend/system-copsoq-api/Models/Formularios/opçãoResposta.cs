using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.Models.Formularios
{
    public class OpcaoResposta
    {
        public int ID { get; set; }

        [Required]
        public string Texto { get; set; } = string.Empty; // Ex: "Sempre", "Concordo", "Nunca"

        [Required]
        public int Valor { get; set; } // O valor numérico (ex: 6, 5, 1, 0)

        public int Ordem { get; set; } // Para ordenar (ex: "Sempre" (valor 6) pode ser a opção 1)

        // --- Chave Estrangeira ---
        // A que Questionário esta opção pertence
        [Required]
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!;
    }
}