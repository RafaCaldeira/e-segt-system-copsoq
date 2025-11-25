// --- TIPOS PARA RESPONDER (O funcionário vê isto) ---

export interface PerguntaRespostaDto {
  id: number;
  texto: string;
}

export interface OpcaoRespostaDto {
  texto: string;
  valor: number;
  ordem: number;
}

export interface DimensaoRespostaDto {
  id: number;
  titulo: string;
  ordem: number;
  perguntas: PerguntaRespostaDto[];
}

export interface FuncionarioSimplesDto {
  nome: string;
  setor: string;
  cpf: string;
  nomeEmpresa: string;
}

export interface QuestionarioParaResponderDto {
  id: number;
  titulo: string;
  textoIntroducao: string;
  textoConsentimento: string;
  dimensoes: DimensaoRespostaDto[];
  funcionario: FuncionarioSimplesDto;
  opcoesResposta: OpcaoRespostaDto[]; 
}

export interface QuestionarioCreateDto {
  titulo: string;
  descricao: string;
  textoIntroducao: string;
  textoConsentimento: string;
  setoresAplicaveis: string[];
}

export interface OpcaoRespostaCreateDto {
  texto: string;
  valor: number;
  ordem: number;
}

export interface DimensaoCreateDto {
  titulo: string;
  nomeIndicador: string;
  ordem: number;
}

export interface PerguntaCreateDto {
  texto: string;
}

// Tipo base do Questionário (retornado após criar)
export interface Questionario {
  id: number;
  titulo: string;
  descricao: string;
  // ... outros campos se necessário
}