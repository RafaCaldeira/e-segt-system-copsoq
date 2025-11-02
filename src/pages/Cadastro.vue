<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiService } from '../services/api.service';
import type { RegistroClienteDto } from '../types/auth.types';
import { SetorAtuacao } from '../types/setor.types'; // Para o <select>

// Refs para o formulário
const formData = ref<RegistroClienteDto>({
  email: '',
  senha: '',
  nomeEmpresa: '',
  nomeResponsavel: '',
  setorAtuacao: SetorAtuacao.Industria, // Valor padrão
  cidade: '',
  cnpj: ''
});
const confirmarSenha = ref(''); // Campo extra para validação no front-end

// Refs de estado
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const router = useRouter();

// Opções para o <select>
const setores = ref(Object.values(SetorAtuacao)); // Converte o enum num array ["Industria", "Comercio", ...]

async function handleRegister() {
  if (isLoading.value) return;

  // Validação simples no front-end
  if (formData.value.senha !== confirmarSenha.value) {
    errorMessage.value = 'As senhas não coincidem.';
    return;
  }
  
  // (Poderia adicionar mais validações aqui, mas a API já trata da senha)
  
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await apiService.registerCliente(formData.value);

    if (response.success) {
      // SUCESSO!
      alert('Registo efetuado com sucesso! Por favor, faça o login.');
      router.push('/login'); // Redireciona para o login
    } else {
      // Exibe o erro vindo da API
      errorMessage.value = response.error || 'Erro no registo.';
    }
  } catch (error) {
    errorMessage.value = 'Ocorreu um erro inesperado.';
  }

  isLoading.value = false;
}
</script>

<template>
  <div class="login-container"> <form @submit.prevent="handleRegister" class="login-form"> <h1>Registo de Cliente</h1>
      
      <div class="form-group">
        <label for="email">Email</label>
        <input type="email" id="email" v-model="formData.email" required>
      </div>

      <div class="form-group">
        <label for="nomeEmpresa">Nome da Empresa</label>
        <input type="text" id="nomeEmpresa" v-model="formData.nomeEmpresa" required>
      </div>

      <div class="form-group">
        <label for="nomeResponsavel">Nome do Responsável</label>
        <input type="text" id="nomeResponsavel" v-model="formData.nomeResponsavel" required>
      </div>

      <div class="form-group">
        <label for="cnpj">CNPJ</label>
        <input type="text" id="cnpj" v-model="formData.cnpj" required>
      </div>

      <div class="form-group">
        <label for="cidade">Cidade</label>
        <input type="text" id="cidade" v-model="formData.cidade" required>
      </div>

      <div class="form-group">
        <label for="setor">Setor de Atuação</label>
        <select id="setor" v-model="formData.setorAtuacao" required>
          <option v-for="setor in setores" :key="setor" :value="setor">
            {{ setor }}
          </option>
        </select>
      </div>
      
      <div class="form-group">
        <label for="password">Senha</label>
        <input type="password" id="password" v-model="formData.senha" required>
        <small>(Mín. 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 especial)</small>
      </div>

      <div class="form-group">
        <label for="confirmPassword">Confirmar Senha</label>
        <input type="password" id="confirmPassword" v-model="confirmarSenha" required>
      </div>

      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <button type="submit" :disabled="isLoading">
        {{ isLoading ? 'A registar...' : 'Registar' }}
      </button>

      <p class="register-link">
        Já tem uma conta? 
        <router-link to="/login">Faça o login aqui</router-link>
      </p>

    </form>
  </div>
</template>

<style scoped>
/* Importamos os estilos do Login.vue para consistência */
/* (Se você moveu os estilos do Login para um CSS global, não precisa disto) */

.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 2rem 0; /* Adicionado padding para formulários longos */
  min-height: 100vh;
}

.login-form {
  width: 100%;
  max-width: 450px; /* Um pouco mais largo para o formulário de registo */
  padding: 2rem;
  border: 1px solid #444;
  border-radius: 8px;
  background-color: #2a2a2a;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
}

h1 {
  text-align: center;
  margin-bottom: 1.5rem;
  color: #fff;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #ccc;
}

.form-group input,
.form-group select { /* Aplicando estilo ao select também */
  width: 100%;
  padding: 0.8rem;
  font-size: 1rem;
  border-radius: 4px;
  border: 1px solid #555;
  background-color: #333;
  color: #fff;
  box-sizing: border-box; 
}

.form-group small {
  font-size: 0.8rem;
  color: #999;
  margin-top: 4px;
  display: block;
}

.error-message {
  color: #ff6b6b;
  margin-bottom: 1rem;
  text-align: center;
}

button {
  width: 100%;
  padding: 0.8rem;
  font-size: 1.1rem;
  font-weight: bold;
  color: #fff;
  background-color: #42b883; 
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