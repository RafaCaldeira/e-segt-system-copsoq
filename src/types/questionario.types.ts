// (Adicionei esta interface, pois a 'DimensaoRespostaDto' precisa dela)
export interface PerguntaRespostaDto {
  id: number;
  texto: string;
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
  funcionario: FuncionarioSimplesDto; // <-- A nova propriedade está incluída
}