// ATENÇÃO: O Status "Em Andamento" deve ter espaço para bater com seu Vue
export type StatusAcao = 'Pendente' | 'EmAndamento' | 'Concluido';

export interface Acao {
  id: number;
  descricao: string;
  prazo?: string;
  status: StatusAcao;
  planoDeAcaoID: number;

  data_conclusao?: string | null;
  justificativa?: string | null;
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
  status?: string; 
}

export interface PlanoDeAcaoCreateDto {
  titulo: string;
  descricao: string;
  empresaID: number;
}