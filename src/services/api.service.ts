import type { QuestionarioParaResponderDto } from '../types/questionario.types';
import type { SubmissaoDto } from '../types/submissao.types';
import type { LoginSuccessResponse, AuthResponse, RegistroClienteDto } from '../types/auth.types';
import type { Funcionario, FuncionarioCreateDto } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types';
import type { RelatorioCompletoDto } from '../types/relatorio.types';
// IMPORTE OS NOVOS TIPOS
import type { PlanoDeAcao, PlanoDeAcaoCreateDto, AcaoCreateDto, Acao } from '../types/plano.types';
import type { OpcaoRespostaCreateDto } from '../types/questionario.types';
import type { DisparoCreateDto } from '../types/disparo.types';
import type { Questionario } from '../types/questionario.types';
import type { DisparoHistoricoDto } from '../types/disparo.types';



const API_BASE_URL = 'http://localhost:5258/api';

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem('user-token'); 
  if (token) {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  } else {
    return { 'Content-Type': 'application/json' };
  }
}

export const apiService = {
  
  // --- Métodos Públicos ---
  async getQuestionarioParaResponder(token: string): Promise<QuestionarioParaResponderDto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/responder/${token}`);
      if (!response.ok) return null;
      const data = await response.json();
      return data; // (Adicione a limpeza de $id/$ref aqui se necessário)
    } catch (error) {
      return null;
    }
  },

  async submitRespostas(token: string, submissao: SubmissaoDto): Promise<{ success: boolean, message?: string }> {
    try {
      const response = await fetch(`${API_BASE_URL}/responder/${token}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(submissao),
      });

      if (!response.ok) {
        const errorText = await response.text();
        try {
            // Attempt to parse backend JSON error message
            const errorJson = JSON.parse(errorText);
            // Look for 'message' or 'title' common in .NET errors
            const msg = errorJson.message || errorJson.title || errorText; 
            return { success: false, message: msg };
        } catch {
            // Fallback to raw text
            return { success: false, message: errorText || 'Erro no servidor' };
        }
      }

      return { success: true };
    } catch (error) {
      return { success: false, message: 'Falha de conexão com a API.' };
    }
  },

  async login(email: string, senha: string): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, senha }),
      });
      if (!response.ok) return { success: false, token: null, userRole: null, error: 'Credenciais inválidas' };
      const data: LoginSuccessResponse = await response.json();
      return { 
        success: true, 
        token: data.token, 
        userRole: data.userRole, 
        nomeEmpresa: data.nomeEmpresa, 
        empresaId: data.empresaId,
        // ADICIONE ESTA LINHA para passar o ID para a store
        id: data.id || data.userId 
      };
    } catch (error) {
      return { success: false, token: null, userRole: null, error: 'Falha de rede' };
    }
  },

  async registerCliente(data: RegistroClienteDto): Promise<{ success: boolean; error?: string }> {
    // ... (o seu código de registo existente) ...
    // (Vou omitir para poupar espaço, mas mantenha-o!)
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
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      if (response.status === 201) return { success: true };
      const errorData = await response.json();
      if (response.status === 409) return { success: false, error: 'Este email já está a ser utilizado.' };
      if (response.status === 400) {
        const firstError = errorData.errors ? Object.values(errorData.errors)[0] : 'Dados inválidos.';
        return { success: false, error: (firstError as string[])[0] || 'Dados inválidos.' };
      }
      return { success: false, error: 'Ocorreu um erro desconhecido.' };
    } catch (error) {
      return { success: false, error: 'Falha de rede' };
    }
  },

  // --- Dashboard / Funcionários ---
  async getFuncionarios(): Promise<Funcionario[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/funcionario`, { method: 'GET', headers: getAuthHeaders() });
      if (!response.ok) return null;
      const data = await response.json();
      // Limpeza simples
      return JSON.parse(JSON.stringify(data), (k, v) => {
         if (v && v.$values) return v.$values;
         if (k === '$id' || k === '$ref') return undefined;
         return v;
      }) as Funcionario[];
    } catch (error) { return null; }
  },

  async getEmpresas(): Promise<Empresa[] | null> {
     try {
      const response = await fetch(`${API_BASE_URL}/empresa`, { method: 'GET', headers: getAuthHeaders() });
      if (!response.ok) return null;
      const data = await response.json();
      return JSON.parse(JSON.stringify(data), (k, v) => {
         if (v && v.$values) return v.$values;
         if (k === '$id' || k === '$ref') return undefined;
         return v;
      }) as Empresa[];
    } catch (error) { return null; }
  },

  // --- Relatórios ---
  async getRelatorio(empresaId: number, questionarioId: number): Promise<RelatorioCompletoDto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/relatorio/empresa/${empresaId}/questionario/${questionarioId}`, {
        method: 'GET',
        headers: getAuthHeaders()
      });
      if (!response.ok) return null;
      const data = await response.json();
      return JSON.parse(JSON.stringify(data), (k, v) => {
         if (v && v.$values) return v.$values;
         if (k === '$id' || k === '$ref') return undefined;
         return v;
      }) as RelatorioCompletoDto;
    } catch (error) { return null; }
  },

  // **********************************
  // *** MÉTODOS NOVOS (PLANO DE AÇÃO) ***
  // **********************************

  async getPlanosPorEmpresa(empresaId: number): Promise<PlanoDeAcao[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/planodeacao/empresa/${empresaId}`, {
        method: 'GET',
        headers: getAuthHeaders()
      });
      if (!response.ok) return null;
      
      const data = await response.json();
      return JSON.parse(JSON.stringify(data), (k, v) => {
        if (v && v.$values) return v.$values;
        if (k === '$id' || k === '$ref') return undefined;
        return v;
      }) as PlanoDeAcao[];
    } catch (error) {
      console.error('Erro ao buscar planos:', error);
      return null;
    }
  },

  async createPlano(dto: PlanoDeAcaoCreateDto): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/planodeacao`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      return response.ok;
    } catch (error) {
      return false;
    }
  },

  async addAcao(planoId: number, dto: AcaoCreateDto): Promise<Acao | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/planodeacao/${planoId}/acao`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      if (!response.ok) return null;
      return await response.json();
    } catch (error) {
      return null;
    }
  },

  async updateStatusAcao(acaoId: number, novoStatus: string): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/planodeacao/acao/${acaoId}/status`, {
        method: 'PUT',
        headers: {
            ...getAuthHeaders(), // Mantém o token
            'Content-Type': 'application/json' // Garante que o C# entenda que é JSON
        },
        // O C# espera: { "Status": "Concluido" }
        // Antes estava enviando apenas: "Concluido"
        body: JSON.stringify({ status: novoStatus }) 
      });
      return response.ok;
    } catch (error) {
      console.error('Erro ao atualizar status:', error);
      return false;
    }
  },

  // **********************************
  // *** MÉTODOS NOVOS (Criar Formulário) ***
  // **********************************

  async createQuestionario(data: any) {
  try {
    const response = await fetch(`${API_BASE_URL}/questionario`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });

    if (!response.ok) return null;
    return await response.json();
  } catch (error) {
    return null;
  }
}, 

  async createDimensao(questionarioId: number, data: any) {
  try {
    const response = await fetch(`${API_BASE_URL}/questionario/${questionarioId}/dimensao`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });

    if (!response.ok) return null;
    return await response.json();
  } catch (error) {
    return null;
  }
},

async createPergunta(questionarioId: number, dimensaoId: number, data: any) {
  try {
    const response = await fetch(`${API_BASE_URL}/questionario/${questionarioId}/dimensao/${dimensaoId}/pergunta`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });

    if (!response.ok) return null;
    return await response.json();
  } catch (error) {
    return null;
  }
},

async createOpcaoResposta(questionarioId: number, dto: OpcaoRespostaCreateDto): Promise<boolean> {
    try {
      // A URL é .../questionario/{id}/opcao (sem perguntaId)
      const response = await fetch(`${API_BASE_URL}/questionario/${questionarioId}/opcao`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      return response.ok;
    } catch (error) {
      console.error('Erro ao criar opção:', error);
      return false;
    }
  },
async importarFuncionariosCsv(file: File): Promise<{ success: boolean; message: string; erros?: string[] }> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(`${API_BASE_URL}/funcionario/importar`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('user-token')}`
          // Nota: NÃO defina 'Content-Type' aqui! O browser define automaticamente como 'multipart/form-data' com o boundary correto.
        },
        body: formData
      });

      const data = await response.json();

      if (response.ok) {
        return { success: true, message: data.message, erros: data.erros };
      } else {
        return { success: false, message: data.title || 'Erro ao importar ficheiro.' };
      }
    } catch (error) {
      console.error('Erro no upload:', error);
      return { success: false, message: 'Falha na comunicação com a API.' };
    }
  },


// **********************************
// *** MÉTODOS NOVOS (Criar Fcuncionário) ***
// **********************************

async createFuncionario(dto: FuncionarioCreateDto): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/funcionario`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      return response.ok;
    } catch (error) {
      console.error('Erro ao criar funcionário:', error);
      return false;
    }
  },

  async updateFuncionario(id: number, dto: FuncionarioCreateDto): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/funcionario/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      return response.ok;
    } catch (error) {
      console.error('Erro ao atualizar funcionário:', error);
      return false;
    }
  },

  async deleteFuncionario(id: number): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/funcionario/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
      });
      return response.ok; // 204 No Content
    } catch (error) {
      console.error('Erro ao excluir funcionário:', error);
      return false;
    }
  },

async getFuncionarioById(id: number): Promise<Funcionario | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/funcionario/${id}`, {
      method: 'GET',
      headers: getAuthHeaders()
    });

    if (!response.ok) return null;

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Erro ao buscar funcionário por ID:', error);
    return null;
  }
},

async getQuestionarios(): Promise<Questionario[] | null> {
    try {
      // Assumindo que você tem um endpoint GET /api/questionario que lista todos
      const response = await fetch(`${API_BASE_URL}/questionario`, {
        method: 'GET',
        headers: getAuthHeaders()
      });
      if (!response.ok) return null;
      const data = await response.json();
      // Limpeza básica de $values se necessário
      return JSON.parse(JSON.stringify(data), (_key, v) => {
         if (v && v.$values) return v.$values;
         return v;
      }) as Questionario[];
    } catch (error) { return null; }
},

  // 2. Buscar Funcionários de uma Empresa específica (Para o Admin)
  // (Se o seu backend GetFuncionarios já retorna TUDO para o admin, podemos filtrar no front,
  // mas o ideal seria um endpoint /api/funcionario/empresa/{id})
async getFuncionariosPorEmpresaId(empresaId: number): Promise<Funcionario[] | null> {
    try {
      // Nota: Se este endpoint não existir no backend, teremos de usar o getFuncionarios()
      // e filtrar no JavaScript. Vou assumir que filtramos no front por enquanto.
      const allFuncs = await this.getFuncionarios(); 
      if (!allFuncs) return null;
      return allFuncs.filter(f => f.empresaID === empresaId);
    } catch (error) { return null; }
},
// 3. Disparar (Enviar)
  async createDisparo(dto: DisparoCreateDto): Promise<{ success: boolean; message: string }> {
    try {
      const response = await fetch(`${API_BASE_URL}/disparo`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
      });
      
      const data = await response.json();
      if (response.ok) {
        return { success: true, message: data.message || 'Enviado com sucesso!' };
      } else {
        return { success: false, message: data.title || 'Erro ao enviar.' };
      }
    } catch (error) {
      return { success: false, message: 'Falha de rede.' };
    }
  },



  async getEmpresasParaPsicologo(): Promise<Empresa[] | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/empresa/para-psicologo`, {
      method: 'GET',
      headers: getAuthHeaders()
    });

    if (!response.ok) return null;

    const data = await response.json();
    return JSON.parse(JSON.stringify(data), (_key, val) => {
      if (val?.$values) return val.$values;
      return val;
    });
  } catch (error) {
    console.error("Erro ao carregar empresas:", error);
    return null;
  }
},

async getFuncionariosComStatus(empresaId: number): Promise<any[] | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/funcionario/empresa/${empresaId}/funcionarios`,{
      method: 'GET',
      headers: getAuthHeaders()
    });

    if (!response.ok) return null;

    const data = await response.json();
    return JSON.parse(JSON.stringify(data), (_key, val) => {
      if (val?.$values) return val.$values;
      return val;
    });
  } catch (error) {
    console.error("Erro ao carregar funcionários:", error);
    return null;
  }
},

async getRespostasDetalhadas(funcionarioId: number): Promise<any | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/relatorio/respostas/${funcionarioId}`, {
      method: 'GET',
      headers: getAuthHeaders()
    });

    if (!response.ok) return null;

    return await response.json();
  } catch (error) {
    console.error("Erro ao buscar respostas detalhadas:", error);
    return null;
  }
}, 

async getListaFuncionarios(empresaId: number): Promise<any[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/empresa/${empresaId}/lista-funcionarios`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) return null;

      const data = await response.json();
      
      // Limpeza do JSON ($id, $ref, $values)
      return JSON.parse(JSON.stringify(data), (_key, v) => {
         if (v && v.$values) return v.$values;
         if (_key === '$id' || _key === '$ref') return undefined;
         return v;
      });
    } catch (error) {
      console.error('Erro ao buscar lista de funcionários:', error);
      return null;
    }
  },

  // **********************************
  // *** MÉTODOS NOVOS (Histórico) ***
  // **********************************
  async getHistoricoDisparos(): Promise<DisparoHistoricoDto[] | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/disparo/historico`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) return null;

      const data = await response.json();

      // Limpeza padrão para garantir que arrays do .NET ($values) sejam lidos corretamente
      return JSON.parse(JSON.stringify(data), (k, v) => {
        if (v && v.$values) return v.$values;
        if (k === '$id' || k === '$ref') return undefined;
        return v;
      }) as DisparoHistoricoDto[];
      
    } catch (error) {
      console.error('Erro ao buscar histórico:', error);
      return null;
    }
  },

  async getUsuarioAtual(id: number) {
    try {
      // CORREÇÃO: Usando fetch em vez de api.get
      const response = await fetch(`${API_BASE_URL}/usuario/${id}`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) return null;
      
      return await response.json();
    } catch (error) {
      console.error("Erro ao buscar usuário", error);
      return null;
    }
  },

  async updateUsuario(id: number, data: any) {
    try {
      // CORREÇÃO: Usando fetch em vez de api.put
      const response = await fetch(`${API_BASE_URL}/usuario/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(data)
      });

      return response.ok;
    } catch (error) {
      console.error("Erro ao atualizar usuário", error);
      return false;
    }
  }
};