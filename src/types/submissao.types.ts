import type { RespostaDto } from './resposta.types';

export interface SubmissaoDto {
  cpf: string;
  respostas: RespostaDto[];
}