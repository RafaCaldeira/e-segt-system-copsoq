export interface PerguntaRespostaDto {
  id: number;
  texto: string;
}

export interface DimensaoRespostaDto {
  id: number;
  titulo: string;
  ordem: number;
  // O JSON pode vir como { $id: "...", $values: [...] }
  // O ideal é tratar isso no serviço, mas por agora mantemos a interface simples
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
}