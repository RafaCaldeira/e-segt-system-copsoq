// Importa dos tipos na pasta 'types'
import type { QuestionarioParaResponderDto } from '../types/questionario.types';
import type { LoginSuccessResponse, AuthResponse, RegistroClienteDto } from '../types/auth.types';
import type { SubmissaoDto } from '../types/submissao.types';
import { useUserStore } from '../store/user';
import type { Funcionario } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types';

const API_BASE_URL = 'http://localhost:5258/api';

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('user-token'); 

  if (token) {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}` // <-- Adiciona o Token Bearer
    };
  } else {
    return {
      'Content-Type': 'application/json',
    };
  }
}

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

  async submitRespostas(token: string, submissao: SubmissaoDto): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/responder/${token}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(submissao),
      });

      if (!response.ok) {
        // Ex: 400 (Já respondido) ou 404 (Token não encontrado)
        console.error(`Erro ao enviar respostas: ${response.status} ${response.statusText}`);
        return false;
      }

      // 200 OK (com a mensagem de sucesso)
      return true;

    } catch (error) {
      console.error('Falha na comunicação com a API ao enviar:', error);
      return false;
    }
  },

  async login (email: string, senha:string): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, senha }),
      });

      if (!response.ok) {
        // Ex: 401 Unauthorized (email/senha errados)
        return { success: false, token: null, userRole: null, error: 'Credenciais inválidas' };
      }

      const data: LoginSuccessResponse = await response.json();
      return {
        success: true,
        token: data.token,
        userRole: data.userRole,
      };

    } catch (error) {
      console.error('Falha na comunicação com a API ao fazer login:', error);
      return { success: false, token: null, userRole: null, error: 'Falha de rede' };
    }
  },

  async registerCliente(data: RegistroClienteDto): Promise<{success: boolean; error?: string}> {

  const payload = {
      Email: data.email,
      Senha: data.senha,
      NomeEmpresa: data.nomeEmpresa,
      NomeResponsavel: data.nomeResponsavel,
      SetorAtuacao: data.setorAtuacao,
      Cidade: data.cidade,
      Cnpj: data.cnpj
  };

  try {
        const response = await fetch(`${API_BASE_URL}/auth/register-cliente`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
        });

        if (response.status === 201) { // 201 Created
          return { success: true };
        }

        // Tratar erros
        const errorData = await response.json();
        
        if (response.status === 409) { // 409 Conflict (Email já existe)
          return { success: false, error: 'Este email já está a ser utilizado.' };
        }
        
        if (response.status === 400) { // 400 Bad Request (Validação falhou)
          // O 'errorData' pode ter detalhes dos erros de validação
          console.error('Erros de validação:', errorData);
          return { success: false, error: errorData.title || 'Dados inválidos. Verifique a sua senha (mín. 8 caracteres, maiúscula, minúscula, número, especial).' };
        }

        return { success: false, error: 'Ocorreu um erro desconhecido.' };

      } catch (error) {
        console.error('Falha na comunicação com a API ao registar:', error);
        return { success: false, error: 'Falha de rede' };
      }
  },
  
  async getFuncionarios(): Promise<Funcionario[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/funcionario`, {
        method: 'GET',
        headers: getAuthHeaders() // <-- Usa os cabeçalhos autenticados
      });

      if (!response.ok) {
        console.error(`Erro ao buscar funcionários: ${response.status} ${response.statusText}`);
        return null;
      }

      const data: Funcionario[] = await response.json();
      
      // "Limpa" os dados $id e $ref
      const cleanedData = JSON.parse(JSON.stringify(data), (key, value) => {
        if (value && typeof value === 'object' && value.$values) return value.$values;
        if (key === '$id' || key === '$ref') return undefined;
        return value;
      });

      return cleanedData as Funcionario[];

    } catch (error) {
      console.error('Falha na comunicação com a API ao buscar funcionários:', error);
      return null;
    }
  },

  async getEmpresas(): Promise<Empresa[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/empresa`, {
        method: 'GET',
        headers: getAuthHeaders() // <-- Usa os cabeçalhos autenticados
      });

      if (!response.ok) {
        // Ex: 401 (sem token) ou 403 (não é Admin)
        console.error(`Erro ao buscar empresas: ${response.status} ${response.statusText}`);
        return null;
      }

      const data: Empresa[] = await response.json();
      const cleanedData = JSON.parse(JSON.stringify(data), (key, value) => {
        if (value && typeof value === 'object' && value.$values) return value.$values;
        if (key === '$id' || key === '$ref') return undefined;
        return value;
      });
      return cleanedData as Empresa[];
    } catch (error) {
      console.error('Falha na comunicação com a API ao buscar empresas:', error);
      return null;
    }
  }
};