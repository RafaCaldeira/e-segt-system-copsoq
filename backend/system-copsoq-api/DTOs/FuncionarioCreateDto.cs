using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.DTOs
{
    // Este DTO tem APENAS os campos que o front-end envia.
    // Não tem ID, EmpresaID ou o objeto Empresa.
    public class FuncionarioCreateDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O setor é obrigatório.")]
        public string Setor { get; set; } = string.Empty;
    }
}