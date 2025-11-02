<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user'; // <-- 1. Importe a sua store

//refs para os campos do formulário
const email = ref('');
const password = ref('');
const errorMessage = ref<string | null>(null);
const isLoading = ref<boolean>(false);

const router = useRouter(); // Para redirecionar após o login
const userStore = useUserStore(); // 2. Use a store

async function handleLogin() {
  if (isLoading.value) return; // Previne cliques duplos

  isLoading.value = true;
  errorMessage.value = null;

  try {
    // 3. Chame a AÇÃO de login da store
    const success = await userStore.login(email.value, password.value);

    if (success) {
      router.push('/dashboard'); 
    } else {
      errorMessage.value = 'Email ou senha inválidos.';
    }
  } catch (error) {
    errorMessage.value = 'Ocorreu um erro inesperado. Tente novamente.';
  }

  isLoading.value = false;
}
</script>

<template>
  <div class="login-container">
    <form @submit.prevent="handleLogin" class="login-form">
      <h1>Login</h1>
      
      <div class="form-group">
        <label for="email">Email</label>
        <input 
          type="email" 
          id="email" 
          v-model="email" 
          required
        >
      </div>

      <div class="form-group">
        <label for="password">Senha</label>
        <input 
          type="password" 
          id="password" 
          v-model="password" 
          required
        >
      </div>

      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <button type="submit" :disabled="isLoading">
        {{ isLoading ? 'A entrar...' : 'Entrar' }}
      </button>

      <p class="register-link">
        Não tem uma conta? 
        <router-link to="/cadastro">Registe-se aqui</router-link>
      </p>
    </form>
  </div>
</template>

<style scoped>
/* Estilos básicos para o formulário de login */
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 80vh;
  /* (Presume que 'style.css' tem estilos globais de fundo) */
}

.login-form {
  width: 100%;
  max-width: 400px;
  padding: 2rem;
  border: 1px solid #444;
  border-radius: 8px;
  background-color: #2a2a2a; /* Cor de fundo escura, ajuste conforme necessário */
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
}

h1 {
  text-align: center;
  margin-bottom: 1.5rem;
  color: #fff; /* Cor do texto clara */
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #ccc; /* Cor do rótulo clara */
}

.form-group input {
  width: 100%;
  padding: 0.8rem;
  font-size: 1rem;
  border-radius: 4px;
  border: 1px solid #555;
  background-color: #333;
  color: #fff;
  box-sizing: border-box; /* Garante que o padding não quebre o layout */
}

.error-message {
  color: #ff6b6b; /* Vermelho para erros */
  margin-bottom: 1rem;
  text-align: center;
}

button {
  width: 100%;
  padding: 0.8rem;
  font-size: 1.1rem;
  font-weight: bold;
  color: #fff;
  background-color: #42b883; /* Verde Vue */
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.2s;
}

button:hover {
  background-color: #369a6e;
}

button:disabled {
  background-color: #555;
  cursor: not-allowed;
}

.register-link {
  margin-top: 1.5rem;
  text-align: center;
  font-size: 0.9rem;
  color: #ccc;
}

.register-link a {
  color: #42b883;
  text-decoration: none;
  font-weight: bold;
}
</style>