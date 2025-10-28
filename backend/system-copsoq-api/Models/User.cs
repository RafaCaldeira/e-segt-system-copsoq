using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models;

namespace system_copsoq_api.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; } 

        public int? EmpresaID { get; set; } 
        
        public Empresa? Empresa { get; set; }
    }
}