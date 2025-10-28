using system_copsoq_api.Models; 
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    public class RegistroStaffDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        // (Pode colar a validação de Regex da senha aqui se quiser)
        public string Senha { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; } // O front-end dirá se é 'Administrador' ou 'Psicologa'
    }
}