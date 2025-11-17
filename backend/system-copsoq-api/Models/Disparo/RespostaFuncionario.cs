using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models.Formularios; // Para aceder a 'Pergunta'

// O namespace DEVE refletir a pasta
namespace system_copsoq_api.Models.Disparo 
{
    public class RespostaFuncionario
    {
        public int ID { get; set; }

        [Required]
        public int DisparoID { get; set; } 
        public Disparo Disparo { get; set; } = null!; // Encontrado no mesmo namespace

        [Required]
        public int PerguntaID { get; set; } 
        public Pergunta Pergunta { get; set; } = null!; // Vem de ...Models.Formularios

        [Required]
        public int ValorResposta { get; set; } 
    }
}