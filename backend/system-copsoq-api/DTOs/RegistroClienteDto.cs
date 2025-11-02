using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models;

namespace system_copsoq_api.DTOs
{

    public class RegistroClienteDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#_-])[A-Za-z\d@$!%*?&#_-]{8,}$",
            ErrorMessage = "A senha deve ter no mínimo 8 caracteres, uma letra maiúscula, uma minúscula, um número e um caractere especial."
        )]
        public string Senha { get; set; } = string.Empty;

        // --- Dados para a Empresa ---
        [Required]
        public string NomeEmpresa { get; set; } = string.Empty;
        public string NomeResponsavel { get; set; } = string.Empty;
        public SetorAtuacao SetorAtuacao { get; set; }
        public string Cidade { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
    }
}