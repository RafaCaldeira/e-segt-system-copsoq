using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models.Formularios;

namespace system_copsoq_api.Models.Disparo 
{
    public class RespostaFuncionario
    {
        public int ID { get; set; }

        [Required]
        public int DisparoID { get; set; } 
        public Disparo Disparo { get; set; } = null!;

        [Required]
        public int PerguntaID { get; set; } 
        public Pergunta Pergunta { get; set; } = null!;

        // ADICIONE O '?' AQUI 👇 e remova o [Required]
        public int? ValorResposta { get; set; } 

        public string? TextoResposta { get; set; }
    }
}