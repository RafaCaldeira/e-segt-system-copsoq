using System;
using System.Collections.Generic;

namespace system_copsoq_api.DTOs.Dashboard
{
    // Este DTO é o "pacote" completo que a API devolve
    public class RelatorioCompletoDto
    {
        // --- NOVO CAMPO ---
        public string NomeEmpresa { get; set; } = string.Empty; 

        public string TituloQuestionario { get; set; } = string.Empty;
        public int TotalRespondentes { get; set; }
        public DateTime DataGeracaoRelatorio { get; set; }
        
        // A lista de resultados, um para cada Dimensão/Indicador
        public List<ResultadoIndicadorDto> Resultados { get; set; } = new List<ResultadoIndicadorDto>();
    }
}