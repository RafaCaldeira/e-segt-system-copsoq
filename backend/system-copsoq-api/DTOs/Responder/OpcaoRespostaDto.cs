namespace system_copsoq_api.DTOs.Responder
{
    // Representa uma única opção de resposta (ex: "Sempre" = 4)
    public class OpcaoRespostaDto
    {
        public string Texto { get; set; } = string.Empty;
        public int Valor { get; set; }
        public int Ordem { get; set; }
    }
}