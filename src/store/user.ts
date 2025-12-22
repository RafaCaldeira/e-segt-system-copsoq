import { defineStore } from 'pinia'
import { ref, computed } from 'vue';
import { apiService } from '../services/api.service';
import type { AuthResponse } from '../types/auth.types';

export const useUserStore = defineStore('user', () => {

  // --- ESTADO (STATE) ---
  const token = ref<string | null>(localStorage.getItem('user-token') || null);
  const userRole = ref<string | null>(localStorage.getItem('user-role') || null);
  const nomeEmpresa = ref<string | null>(localStorage.getItem('user-nome-empresa') || null);
  
  // Parse para garantir que é número ou null
  const empresaId = ref<number | null>(JSON.parse(localStorage.getItem('user-empresa-id') || 'null'));
  
  // 1. ADICIONADO: Estado do ID do Usuário
  const userId = ref<number | null>(JSON.parse(localStorage.getItem('user-id') || 'null'));

  // --- GETTERS (COMPUTED) ---
  const isLoggedIn = computed<boolean>(() => !!token.value);
  const isAdmin = computed<boolean>(() => userRole.value === 'Admin'); 
  const isCliente = computed<boolean>(() => userRole.value === 'Cliente');

  // --- ACTIONS ---
  async function login(email: string, senha: string): Promise<boolean> {
    const response: AuthResponse = await apiService.login(email, senha);

    if (response.success && response.token && response.userRole) {
      // 2. Atualizar o estado (state)
      token.value = response.token;
      userRole.value = response.userRole;
      nomeEmpresa.value = response.nomeEmpresa || null;
      empresaId.value = response.empresaId || null;
      
      // Salva o ID do usuário no estado (verifique se sua API retorna 'id' ou 'userId')
      // Estou assumindo 'id' baseado no padrão comum
      userId.value = response.id || null; 

      // 3. Guardar no localStorage
      localStorage.setItem('user-token', response.token);
      localStorage.setItem('user-role', response.userRole);
      
      if (response.nomeEmpresa) {
        localStorage.setItem('user-nome-empresa', response.nomeEmpresa);
      } else {
        localStorage.removeItem('user-nome-empresa');
      }
      
      if (response.empresaId) {
        localStorage.setItem('user-empresa-id', JSON.stringify(response.empresaId));
      } else {
        localStorage.removeItem('user-empresa-id');
      }

      // Salva o ID do usuário no localStorage
      if (response.id) {
        localStorage.setItem('user-id', JSON.stringify(response.id));
      } else {
        localStorage.removeItem('user-id');
      }

      return true;
    } else {
      // Garante que o estado antigo é limpo em caso de falha
      logout();
      return false;
    }
  }

  function logout() {
    token.value = null;
    userRole.value = null;
    nomeEmpresa.value = null;
    empresaId.value = null;
    userId.value = null; // Limpa o ID no estado

    localStorage.removeItem('user-token');
    localStorage.removeItem('user-role');
    localStorage.removeItem('user-nome-empresa');
    localStorage.removeItem('user-empresa-id');
    localStorage.removeItem('user-id'); // Remove do localStorage
  }

  return {
    token,
    userRole,
    nomeEmpresa,
    empresaId,
    userId, // 4. IMPORTANTE: Agora o componente consegue ler isso!
    isLoggedIn,
    isAdmin,
    isCliente,
    login,
    logout,
  };

})