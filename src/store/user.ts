import { defineStore } from 'pinia'
import { ref, computed } from 'vue';
import { apiService } from '../services/api.service';
import type { AuthResponse } from '../types/auth.types';

export const useUserStore = defineStore('user', () => {

  const token = ref<string | null>(localStorage.getItem('user-token') || null);
  const userRole = ref<string | null>(localStorage.getItem('user-role') || null);
  const nomeEmpresa = ref<string | null>(localStorage.getItem('user-nome-empresa') || null);

  const isLoggedIn = computed<boolean>(() => !!token.value);
  const isAdmin = computed<boolean>(() => userRole.value === 'Admin'); // Assumindo 'Admin'
  const isCliente = computed<boolean>(() => userRole.value === 'Cliente');

  async function login(email: string, senha: string): Promise<boolean> {
    const response: AuthResponse = await apiService.login(email, senha);

    if (response.success && response.token && response.userRole) {
      // 1. Atualizar o estado (state)
      token.value = response.token;
      userRole.value = response.userRole;
      nomeEmpresa.value = response.nomeEmpresa || null;

      // 2. Guardar no localStorage
      localStorage.setItem('user-token', response.token);
      localStorage.setItem('user-role', response.userRole);
      
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
    localStorage.removeItem('user-token');
    localStorage.removeItem('user-role');
    localStorage.removeItem('user-nome-empresa');
    // (Aqui também redirecionaríamos para /login, mas faremos isso no router)
  }

  return {
    token,
    userRole,
    nomeEmpresa,
    isLoggedIn,
    isAdmin,
    isCliente,
    login,
    logout,
  };

})