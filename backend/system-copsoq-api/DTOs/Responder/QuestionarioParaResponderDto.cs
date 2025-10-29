using System.Collections.Generic;

namespace system_copsoq_api.DTOs.Responder
{
    // O pacote completo do questionário a ser respondido
    public class QuestionarioParaResponderDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string TextoIntroducao { get; set; } = string.Empty;
        public string TextoConsentimento { get; set; } = string.Empty;
        public List<DimensaoRespostaDto> Dimensoes { get; set; } = new List<DimensaoRespostaDto>();
    }
}