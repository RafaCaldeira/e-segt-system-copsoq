<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user'; 

// --- ESTADO ---
const email = ref('');
const password = ref('');
const errorMessage = ref<string | null>(null);
const isLoading = ref<boolean>(false);

const router = useRouter();
const userStore = useUserStore();

// --- AÇÕES ---
async function handleLogin() {
  if (isLoading.value) return; 

  isLoading.value = true;
  errorMessage.value = null;

  try {
    // Tenta fazer o login na Store
    const success = await userStore.login(email.value, password.value);

    if (success) {
      // --- LÓGICA DE REDIRECIONAMENTO CORRIGIDA ---
      
      if (userStore.userRole === 'Cliente') {
        // 1. Cliente vai para sua área de funcionários
        router.push('/funcionario'); 
      } 
      else if (userStore.userRole === 'Psicologo') {
        // 2. Psicólogo vai para a Área do Psicólogo (ou Dashboard, se preferir)
        router.push('/psicologo'); 
      } 
      else {
        // 3. Admin (e outros) vão para o Dashboard Principal
        router.push('/dashboard'); 
      }

    } else {
      errorMessage.value = 'Email ou senha inválidos.';
    }
  } catch (error) {
    console.error(error);
    errorMessage.value = 'Erro de conexão. Verifique sua internet.';
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="login-wrapper">
    <div class="login-card">
      
      <div class="header">
        <h1>Bem-vindo de volta</h1>
        <p class="subtitle">Insira suas credenciais para acessar</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        
        <div class="form-group">
          <label for="email">Email</label>
          <input 
            type="email" 
            id="email" 
            v-model="email" 
            required
            placeholder="exemplo@email.com"
            autocomplete="email"
          >
        </div>

        <div class="form-group">
          <label for="password">Senha</label>
          <input 
            type="password" 
            id="password" 
            v-model="password" 
            required
            placeholder="••••••••"
            autocomplete="current-password"
          >
        </div>

        <div v-if="errorMessage" class="error-alert">
          ⚠️ {{ errorMessage }}
        </div>

        <button type="submit" :disabled="isLoading" class="btn-login">
          <span v-if="isLoading" class="spinner"></span>
          {{ isLoading ? 'Entrando...' : 'Entrar' }}
        </button>

        <div class="footer-links">
          <p>
            Não tem uma conta? 
            <router-link to="/cadastro">Cadastre-se</router-link>
          </p>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
/* --- RESET GLOBAL PARA ESTA PÁGINA --- */
:global(body) {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  background-color: #f0f2f5;
  font-family: 'Segoe UI', sans-serif;
}

/* --- WRAPPER DE TELA CHEIA --- */
.login-wrapper {
  height: 100vh; 
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #f0f2f5;
}

/* --- O CARD DE LOGIN --- */
.login-card {
  width: 100%;
  max-width: 400px;
  padding: 2.5rem;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05); 
}

/* --- ESTILOS DO CONTEÚDO --- */
.header { text-align: center; margin-bottom: 2rem; }
h1 { margin: 0 0 0.5rem 0; color: #1a1a1a; font-size: 1.8rem; }
.subtitle { color: #666; font-size: 0.95rem; margin: 0; }

.form-group { margin-bottom: 1.2rem; }
.form-group label { display: block; margin-bottom: 0.5rem; color: #333; font-weight: 600; font-size: 0.9rem; }

.form-group input {
  width: 100%; padding: 0.8rem; font-size: 1rem; border-radius: 6px;
  border: 1px solid #ddd; background-color: #fff; color: #333; box-sizing: border-box;
  transition: border-color 0.2s;
}
.form-group input:focus { outline: none; border-color: #42b883; box-shadow: 0 0 0 3px rgba(66, 184, 131, 0.1); }

.error-alert {
  background-color: #fee2e2; color: #dc2626; padding: 0.8rem; border-radius: 6px;
  margin-bottom: 1.2rem; font-size: 0.9rem; text-align: center; border: 1px solid #fecaca;
}

.btn-login {
  width: 100%; padding: 0.9rem; font-size: 1rem; font-weight: bold; color: #fff;
  background-color: #42b883; border: none; border-radius: 6px; cursor: pointer;
  transition: background-color 0.2s; display: flex; justify-content: center; align-items: center; gap: 10px;
}
.btn-login:hover:not(:disabled) { background-color: #369a6e; }
.btn-login:disabled { background-color: #9ca3af; cursor: not-allowed; }

.footer-links { margin-top: 1.5rem; text-align: center; font-size: 0.9rem; color: #666; }
.footer-links a { color: #42b883; text-decoration: none; font-weight: bold; }
.footer-links a:hover { text-decoration: underline; }

.spinner {
  width: 16px; height: 16px; border: 2px solid #ffffff; border-top-color: transparent;
  border-radius: 50%; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 480px) {
  .login-card { box-shadow: none; background-color: transparent; padding: 1.5rem; }
  .login-wrapper { background-color: #fff; } 
}
</style>