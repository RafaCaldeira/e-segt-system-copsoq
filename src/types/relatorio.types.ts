export interface ResultadoIndicadorDto{
    nomeIndicador: string;
    scorePercentual: number; 
    nivelRisco: string;
}

// Corresponde a DTOs/Dashboard/RelatorioCompletoDto.cs
export interface RelatorioCompletoDto {
  nomeEmpresa: string;
  tituloQuestionario: string;
  totalRespondentes: number;
  dataGeracaoRelatorio: string; 
  resultados: ResultadoIndicadorDto[]; 
}

export interface EmpresaSimplesDto {
  id: number;
  nomeEmpresa: string;
  setorAtuacao: string;
}

export interface QuestionarioRespondidoDto {
  questionarioID: number;
  tituloQuestionario: string;
  dataResposta: string;
  tokenAcesso: string;
}

export interface FuncionarioComStatusDto {
  id: number;
  nome: string;
  cargo: string;
  setor: string;
  questionariosRespondidos: QuestionarioRespondidoDto[];
}

export interface RespostaDetalhadaDto {
  funcionario: string;
  questionario: string;
  dataResposta: string;
  respostas: {
    perguntaID: number;
    textoPergunta: string;
    valorResposta: number;
  }[];
}