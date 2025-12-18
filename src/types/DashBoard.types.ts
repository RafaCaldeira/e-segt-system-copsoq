export interface QuestionarioResumoDto {
  titulo: string;
  dataResposta: string;
  tokenAcesso: string;
}

export interface FuncionarioListaDto {
  id: number;
  nome: string;
  cargo: string; // Agora vamos usar isso
  questionariosRespondidos: QuestionarioResumoDto[]; // E a lista para os botões
}