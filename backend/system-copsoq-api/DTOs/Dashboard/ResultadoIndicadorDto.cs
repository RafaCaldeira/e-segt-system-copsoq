namespace system_copsoq_api.DTOs.Dashboard
{
    public class ResultadoIndicadorDto
    {
        public string NomeIndicador { get; set; } = string.Empty;
        public double ScorePercentual { get; set; } // O valor (ex: 78.5)
        public string NivelRisco { get; set; } = string.Empty; // "Alto", "Médio", "Baixo"
    }
}