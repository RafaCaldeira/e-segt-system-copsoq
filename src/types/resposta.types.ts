export interface RespostaDto {
  perguntaId: number;
  // O MUDANÇA IMPORTANTE: Agora aceita number OU null
  valorResposta: number | null; 
  // NOVO CAMPO: Para as perguntas abertas
  textoResposta?: string; 
}