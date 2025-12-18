<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiService } from '../services/api.service';
import type { RegistroClienteDto } from '../types/auth.types';
import { SetorAtuacao } from '../types/setor.types';

// --- ESTADO ---
const formData = ref<RegistroClienteDto>({
  email: '',
  senha: '',
  nomeEmpresa: '',
  nomeResponsavel: '',
  setorAtuacao: SetorAtuacao.Industria,
  cidade: '',
  cnpj: ''
});

const confirmarSenha = ref('');
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);

const router = useRouter();
const setores = ref(Object.values(SetorAtuacao));

// --- AÇÕES ---
async function handleRegister() {
  if (isLoading.value) return;

  // Validação Básica
  if (formData.value.senha !== confirmarSenha.value) {
    errorMessage.value = 'As senhas não coincidem.';
    return;
  }
  
  isLoading.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  try {
    const response = await apiService.registerCliente(formData.value);

    if (response.success) {
      successMessage.value = 'Conta criada com sucesso! Redirecionando...';
      // Pequeno delay para o usuário ler a mensagem antes de ir para o login
      setTimeout(() => {
        router.push('/login');
      }, 2000);
    } else {
      errorMessage.value = response.error || 'Erro ao criar conta. Verifique os dados.';
    }
  } catch (error) {
    errorMessage.value = 'Erro de conexão com o servidor.';
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="register-wrapper">
    <div class="register-card">
      
      <div class="header">
        <h1>Criar Conta</h1>
        <p class="subtitle">Cadastre sua empresa para começar</p>
      </div>

      <form @submit.prevent="handleRegister" class="register-form">
        
        <div class="section-label">Dados da Empresa</div>
        
        <div class="form-group">
          <label for="nomeEmpresa">Nome da Empresa</label>
          <input 
            type="text" 
            id="nomeEmpresa" 
            v-model="formData.nomeEmpresa" 
            placeholder="Ex: Tech Solutions Ltda"
            required
            autocomplete="organization"
          >
        </div>

        <div class="form-row">
          <div class="form-group half">
            <label for="cnpj">CNPJ</label>
            <input 
              type="text" 
              id="cnpj" 
              v-model="formData.cnpj" 
              placeholder="00.000.000/0000-00"
              required
            >
          </div>

          <div class="form-group half">
            <label for="setor">Setor</label>
            <select id="setor" v-model="formData.setorAtuacao" required>
              <option v-for="setor in setores" :key="setor" :value="setor">
                {{ setor }}
              </option>
            </select>
          </div>
        </div>

        <div class="form-group">
          <label for="cidade">Cidade</label>
          <input 
            type="text" 
            id="cidade" 
            v-model="formData.cidade" 
            placeholder="Ex: São Paulo"
            required
            autocomplete="address-level2"
          >
        </div>

        <div class="section-label">Dados de Acesso</div>

        <div class="form-group">
          <label for="nomeResponsavel">Nome do Responsável</label>
          <input 
            type="text" 
            id="nomeResponsavel" 
            v-model="formData.nomeResponsavel" 
            placeholder="Seu nome completo"
            required
            autocomplete="name"
          >
        </div>

        <div class="form-group">
          <label for="email">Email Corporativo</label>
          <input 
            type="email" 
            id="email" 
            v-model="formData.email" 
            placeholder="voce@empresa.com"
            required
            autocomplete="email"
          >
        </div>

        <div class="form-row">
          <div class="form-group half">
            <label for="password">Senha</label>
            <input 
              type="password" 
              id="password" 
              v-model="formData.senha" 
              placeholder="••••••••"
              required
              autocomplete="new-password"
            >
          </div>

          <div class="form-group half">
            <label for="confirmPassword">Confirmar</label>
            <input 
              type="password" 
              id="confirmPassword" 
              v-model="confirmarSenha" 
              placeholder="••••••••"
              required
              autocomplete="new-password"
            >
          </div>
        </div>
        <small class="password-hint">Mínimo 8 caracteres, 1 maiúscula, 1 número.</small>

        <div v-if="errorMessage" class="alert error">
          ⚠️ {{ errorMessage }}
        </div>
        <div v-if="successMessage" class="alert success">
          ✅ {{ successMessage }}
        </div>

        <button type="submit" :disabled="isLoading" class="btn-register">
          <span v-if="isLoading" class="spinner"></span>
          {{ isLoading ? 'Criando conta...' : 'Registar Empresa' }}
        </button>

        <div class="footer-links">
          <p>
            Já tem uma conta? 
            <router-link to="/login">Faça o login aqui</router-link>
          </p>
        </div>

      </form>
    </div>
  </div>
</template>

<style scoped>
/* --- RESET GLOBAL --- */
:global(body) {
  margin: 0; padding: 0; box-sizing: border-box;
  background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif;
}

/* --- WRAPPER (Scrollável) --- */
.register-wrapper {
  min-height: 100vh; /* Permite crescer se o form for grande */
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center; /* Centraliza se couber na tela */
  background-color: #f0f2f5;
  padding: 2rem 1rem; /* Espaço em cima e embaixo para não colar nas bordas */
}

/* --- CARD --- */
.register-card {
  width: 100%;
  max-width: 500px; /* Um pouco mais largo que o login */
  padding: 2.5rem;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
}

/* --- TEXTOS --- */
.header { text-align: center; margin-bottom: 2rem; }
h1 { margin: 0 0 0.5rem 0; color: #1a1a1a; font-size: 1.8rem; }
.subtitle { color: #666; font-size: 0.95rem; margin: 0; }
.section-label { 
  font-size: 0.85rem; text-transform: uppercase; letter-spacing: 1px; 
  color: #9ca3af; margin: 1.5rem 0 1rem 0; border-bottom: 1px solid #eee; padding-bottom: 5px; 
}
.password-hint { font-size: 0.8rem; color: #6b7280; display: block; margin-top: -10px; margin-bottom: 1.5rem; }

/* --- FORMULÁRIO --- */
.form-group { margin-bottom: 1.2rem; }
.form-row { display: flex; gap: 15px; } /* Coloca inputs lado a lado */
.half { flex: 1; } /* Cada input ocupa 50% */

.form-group label { display: block; margin-bottom: 0.5rem; color: #333; font-weight: 600; font-size: 0.9rem; }

.form-group input, .form-group select {
  width: 100%; padding: 0.8rem; font-size: 1rem; border-radius: 6px;
  border: 1px solid #ddd; background-color: #fff; color: #333; box-sizing: border-box;
  transition: border-color 0.2s;
}
.form-group input:focus, .form-group select:focus { 
  outline: none; border-color: #42b883; box-shadow: 0 0 0 3px rgba(66, 184, 131, 0.1); 
}

/* --- FEEDBACK --- */
.alert { padding: 0.8rem; border-radius: 6px; margin-bottom: 1.2rem; font-size: 0.9rem; text-align: center; }
.alert.error { background-color: #fee2e2; color: #dc2626; border: 1px solid #fecaca; }
.alert.success { background-color: #d1fae5; color: #065f46; border: 1px solid #a7f3d0; }

/* --- BOTÃO --- */
.btn-register {
  width: 100%; padding: 0.9rem; font-size: 1rem; font-weight: bold; color: #fff;
  background-color: #42b883; border: none; border-radius: 6px; cursor: pointer;
  transition: background-color 0.2s; display: flex; justify-content: center; align-items: center; gap: 10px;
}
.btn-register:hover:not(:disabled) { background-color: #369a6e; }
.btn-register:disabled { background-color: #9ca3af; cursor: not-allowed; }

/* --- LINKS --- */
.footer-links { margin-top: 1.5rem; text-align: center; font-size: 0.9rem; color: #666; }
.footer-links a { color: #42b883; text-decoration: none; font-weight: bold; }
.footer-links a:hover { text-decoration: underline; }

/* --- SPINNER --- */
.spinner {
  width: 16px; height: 16px; border: 2px solid #ffffff; border-top-color: transparent;
  border-radius: 50%; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* Responsividade Mobile */
@media (max-width: 480px) {
  .register-card { box-shadow: none; padding: 1.5rem; }
  .form-row { flex-direction: column; gap: 0; } /* Empilha inputs em telas pequenas */
}
</style>