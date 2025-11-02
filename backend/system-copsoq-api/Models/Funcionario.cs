using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using system_copsoq_api.Models;

namespace system_copsoq_api.Models
{
    public class Funcionario
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // O telefone não é obrigatório
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O setor é obrigatório.")]
        public string Setor { get; set; } = string.Empty; // O departamento do funcionário
        public string CPF { get; set; } = string.Empty;

        public int EmpresaID { get; set; }

        public Empresa Empresa { get; set; } = null!;

        public ICollection<Disparo> Disparos { get; set; } = new List<Disparo>();
    }
}