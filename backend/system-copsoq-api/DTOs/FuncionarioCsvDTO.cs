using CsvHelper.Configuration.Attributes;

namespace system_copsoq_api.DTOs
{
    public class FuncionarioCsvDto
    {
        [Name("Nome")] // O nome da coluna no ficheiro CSV
        public string Nome { get; set; } = string.Empty;

        [Name("Email")]
        public string Email { get; set; } = string.Empty;

        [Name("Telefone")]
        public string Telefone { get; set; } = string.Empty;

        [Name("Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Name("Setor")]
        public string Setor { get; set; } = string.Empty;
        
        [Name("CPF")]
        public string CPF { get; set; } = string.Empty;
    }
}