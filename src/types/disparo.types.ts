export interface DisparoCreateDto {
  questionarioID: number;
  funcionarioIDs: number[];
}

export interface DisparoHistoricoDto {
  id: number;
  nomeFuncionario: string;
  emailFuncionario: string;
  tituloQuestionario: string;
  dataEnvio: string;
  respondido: boolean;
  link: string;
  
  // Campos novos para os gráficos
  setor?: string; 
  empresaId: number; 
}