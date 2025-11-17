using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    // DTO para o Admin cadastrar uma opção (ex: "Nunca" = 0)
    public class OpcaoRespostaCreateDto
    {
        [Required]
        public string Texto { get; set; } = string.Empty;

        [Required]
        public int Valor { get; set; }

        [Required]
        public int Ordem { get; set; } // A ordem em que aparece na tela (1º, 2º, 3º...)
    }
}