using System.Collections.Generic;

namespace system_copsoq_api.DTOs.Responder
{
    public class DimensaoRespostaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<PerguntaRespostaDTO> Perguntas { get; set; } = new List<PerguntaRespostaDTO>();
    }
}