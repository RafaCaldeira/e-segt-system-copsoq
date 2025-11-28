using System;

namespace system_copsoq_api.DTOs
{
    public class DisparoHistoricoDto
    {
        public int Id { get; set; }
        public string NomeFuncionario { get; set; } = string.Empty;
        public string EmailFuncionario { get; set; } = string.Empty;
        public string TituloQuestionario { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public bool Respondido { get; set; }
        public DateTime? DataResposta { get; set; }
        public string Link { get; set; } = string.Empty; // O Token/Link
    }
}