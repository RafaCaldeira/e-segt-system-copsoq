using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models.Formularios; // Para a Pergunta

namespace system_copsoq_api.Models
{
    public class RespostaFuncionario
    {
        public int ID { get; set; }

        // --- Chaves Estrangeiras ---
        [Required]
        public int DisparoID { get; set; } // A que envio esta resposta pertence
        public Disparo Disparo { get; set; } = null!;

        [Required]
        public int PerguntaID { get; set; } // A que pergunta esta resposta se refere
        public Pergunta Pergunta { get; set; } = null!;

        // --- A Resposta ---
        // Vamos guardar a resposta como um número (0-4 para Likert5Pontos)
        // Isso facilita os cálculos para os seus relatórios
        [Required]
        public int ValorResposta { get; set; } 
        // Exemplo: Sempre=4, Frequentemente=3, Às vezes=2, Raramente=1, Nunca=0
    }
}