using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using system_copsoq_api.Models.Disparo; // <-- 1. ADICIONAR ESTE USING

namespace system_copsoq_api.Models.Formularios
{
    public class Pergunta
    {
        public int ID { get; set; }

        [Required]
        public string Texto { get; set; } = string.Empty;

        // --- Chaves Estrangeiras ---
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!;

        public int DimensaoID { get; set; }
        public Dimensao Dimensao { get; set; } = null!;

        // 2. Agora 'RespostaFuncionario' é encontrado
        public ICollection<RespostaFuncionario> Respostas { get; set; } = new List<RespostaFuncionario>();
    }
}