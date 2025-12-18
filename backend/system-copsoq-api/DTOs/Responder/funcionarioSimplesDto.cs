namespace system_copsoq_api.DTOs.Responder
{
    // DTO simples com os dados de confirmação do funcionário
    public class FuncionarioSimplesDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;         
        public string Setor { get; set; } = string.Empty;       
        public string NomeEmpresa { get; set; } = string.Empty;
    }
}