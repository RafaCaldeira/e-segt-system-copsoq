export interface ResultadoIndicadorDto{
    nomeIndicador: string;
    scorePercentual: number; 
    nivelRisco: string;
}

// Corresponde a DTOs/Dashboard/RelatorioCompletoDto.cs
export interface RelatorioCompletoDto {
  tituloQuestionario: string;
  totalRespondentes: number;
  dataGeracaoRelatorio: string; 
  resultados: ResultadoIndicadorDto[]; 
}