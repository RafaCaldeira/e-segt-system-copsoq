export type StatusAcao = 'Pendente' | 'EmAndamento' | 'Concluido';

export interface Acao {
  id: number;
  descricao: string;
  prazo?: string;
  status: StatusAcao;
  planoDeAcaoID: number;
}

export interface PlanoDeAcao {
  id: number;
  titulo: string;
  descricao: string;
  dataCriacao: string;
  isAtivo: boolean;
  empresaID: number;
  acoes: Acao[];
}

export interface AcaoCreateDto {
  descricao: string;
  prazo?: string;
}

export interface PlanoDeAcaoCreateDto {
  titulo: string;
  descricao: string;
  empresaID: number;
}