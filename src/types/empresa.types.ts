export type SetorAtuacao = 
  | "Industria"
  | "Comercio"
  | "Saude"
  | "Tecnologia"
  | "Construcao"
  | "ServicosGerais"
  | "Educacao"
  | "Transporte";

  export interface Empresa {
    id: number;
    nomeEmpresa: string;
    nomeResponsavel: string;
    setorAtuacao: SetorAtuacao;
    cidade: string;
    cnpj: string;
    isAtivo: boolean;
  }
