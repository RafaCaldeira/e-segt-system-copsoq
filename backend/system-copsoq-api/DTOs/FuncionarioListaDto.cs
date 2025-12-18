using System;
using System.Collections.Generic;

namespace system_copsoq_api.DTOs.Dashboard
{
    // 1. Este representa cada LINHA da sua tabela (um funcionário)
    public class FuncionarioListaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty; // O campo que faltava

        // Lista dos questionários que este funcionário especificamente já respondeu
        public List<QuestionarioResumoDto> QuestionariosRespondidos { get; set; } = new List<QuestionarioResumoDto>();
    }

    // 2. Este representa os detalhes para o botão/dropdown
    public class QuestionarioResumoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public DateTime? DataResposta { get; set; }
        
        // O Token é CRUCIAL: é ele que você vai usar no link do botão de download do PDF
        public string TokenAcesso { get; set; } = string.Empty; 
    }
}