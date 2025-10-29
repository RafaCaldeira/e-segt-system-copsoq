// Importa dos tipos na pasta 'types'
import type { QuestionarioParaResponderDto } from '../types/questionario.types';

const API_BASE_URL = 'http://localhost:5258/api';

export const apiService = {
  async getQuestionarioParaResponder(token: string): Promise<QuestionarioParaResponderDto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/responder/${token}`);
      if (!response.ok) {
        console.error(`Erro ao buscar questionário: ${response.status} ${response.statusText}`);
        return null;
      }
      const data: QuestionarioParaResponderDto = await response.json();
      // O JSON retornado pela API pode ter os $id e $ref devido ao ReferenceHandler
      // Precisamos de uma função para "limpar" isso se necessário, ou usar bibliotecas
      // que lidem com isso, mas por agora vamos retornar os dados brutos.
      return data;
    } catch (error) {
      console.error('Falha na comunicação com a API:', error);
      return null;
    }
  },

  // (Método POST para enviar respostas virá aqui)
};