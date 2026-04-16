<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiService } from '../services/api.service';
import type { FuncionarioCreateDto } from '../types/funcionario.types';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

const router = useRouter();

// --- ESTADO ---
const form = ref<FuncionarioCreateDto>({
  nome: '',
  email: '',
  cpf: '',
  telefone: '',
  cargo: '',
  setor: ''
});

const isLoading = ref(false);
const message = ref<{ text: string; type: 'success' | 'error' } | null>(null);

// --- HELPERS (MÁSCARAS) ---
function formatCPF(value: string) {
  return value
    .replace(/\D/g, '') 
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})/, '$1-$2')
    .replace(/(-\d{2})\d+?$/, '$1');
}

function formatPhone(value: string) {
  return value
    .replace(/\D/g, '')
    .replace(/^(\d{2})(\d)/g, '($1) $2')
    .replace(/(\d)(\d{4})$/, '$1-$2')
    .substring(0, 15);
}

const handleCpfInput = (e: Event) => {
  const target = e.target as HTMLInputElement;
  form.value.cpf = formatCPF(target.value);
};

const handlePhoneInput = (e: Event) => {
  const target = e.target as HTMLInputElement;
  form.value.telefone = formatPhone(target.value);
};

// --- AÇÕES ---
async function salvarFuncionario() {
  message.value = null;

  if (!form.value.nome || !form.value.email || !form.value.cpf) {
    message.value = { text: "Preencha os campos obrigatórios (*).", type: 'error' };
    return;
  }

  isLoading.value = true;
  
  try {
    const sucesso = await apiService.createFuncionario(form.value);

    if (sucesso) {
      message.value = { text: "Funcionário cadastrado com sucesso!", type: 'success' };
      setTimeout(() => router.push('/funcionario'), 1500);
    } else {
      message.value = { text: "Erro ao cadastrar. Verifique se o email ou CPF já existem.", type: 'error' };
    }
  } catch (error) {
    message.value = { text: "Erro de conexão com o servidor.", type: 'error' };
  } finally {
    isLoading.value = false;
  }
}

function voltar() {
  router.push('/funcionario'); 
}
</script>

<template>
  <div class="app-layout">
    
    <AppSidebar />

    <div class="main-wrapper">
      <main class="main-content">
        <div class="responder-container">
          
          <header class="form-header">
            <h1 class="content-title">Cadastrar Novo Funcionário</h1>
            <p class="subtitle">Preencha os dados abaixo para adicionar um colaborador ao sistema.</p>
          </header>
          
          <div v-if="message" :class="['alert', message.type]">
            {{ message.text }}
          </div>

          <form @submit.prevent="salvarFuncionario" class="form-content">
            
            <div class="form-grid">
              <div class="form-group">
                <label>Nome Completo <span class="req">*</span></label>
                <input v-model="form.nome" type="text" placeholder="Ex: João Silva" required />
              </div>

              <div class="form-group">
                <label>Email Corporativo <span class="req">*</span></label>
                <input v-model="form.email" type="email" placeholder="joao@empresa.com" required />
              </div>

              <div class="form-group">
                <label>CPF <span class="req">*</span></label>
                <input 
                  :value="form.cpf" 
                  @input="handleCpfInput"
                  type="text" 
                  placeholder="000.000.000-00" 
                  maxlength="14" 
                  required 
                />
              </div>

              <div class="form-group">
                <label>Telefone / Celular</label>
                <input 
                  :value="form.telefone"
                  @input="handlePhoneInput" 
                  type="text" 
                  placeholder="(00) 00000-0000" 
                  maxlength="15"
                />
              </div>

              <div class="form-group">
                <label>Cargo / Função</label>
                <input v-model="form.cargo" type="text" placeholder="Ex: Analista Financeiro" />
              </div>

              <div class="form-group">
                <label>Setor / Departamento</label>
                <input v-model="form.setor" type="text" placeholder="Ex: Financeiro" />
              </div>
            </div>

            <div class="buttons-row">
              <button type="button" class="btn-voltar" @click="voltar">Cancelar</button>
              <button type="submit" class="btn-continuar" :disabled="isLoading">
                {{ isLoading ? 'Salvando...' : '💾 Salvar Funcionário' }}
              </button>
            </div>

          </form>
        </div>
      </main>

      <AppFooter />
    </div>

  </div>
</template>

<style scoped>
/* --- FIX DE LAYOUT (Rolagem) --- */
:global(html), :global(body), :global(#app) {
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden; 
}

/* Layout Geral */
:global(body) { background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif; }

.app-layout { 
  display: flex; 
  height: 100%; 
  width: 100%; 
}

/* --- MAIN WRAPPER --- */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow-y: auto;
}

/* --- MAIN CONTENT --- */
.main-content { 
  flex: 1;
  padding: 2rem; 
  display: flex; 
  justify-content: center; 
  align-items: flex-start;
  background-color: #f0f2f5;
}

.responder-container {
  max-width: 850px; 
  width: 100%; 
  padding: 2.5rem 3rem;
  border-radius: 12px; 
  background-color: #ffffff;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
  margin-bottom: 2rem;
}

/* Header */
.form-header { margin-bottom: 2rem; border-bottom: 1px solid #eee; padding-bottom: 1rem; }
.content-title { font-size: 1.8rem; color: #1f2937; margin: 0 0 0.5rem 0; }
.subtitle { color: #6b7280; margin: 0; font-size: 0.95rem; }

/* Alerts */
.alert { padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; text-align: center; font-weight: 500; }
.alert.success { background-color: #d1fae5; color: #065f46; border: 1px solid #a7f3d0; }
.alert.error { background-color: #fee2e2; color: #991b1b; border: 1px solid #fecaca; }

/* Form */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 2rem; }
.form-group { display: flex; flex-direction: column; }
.form-group label { font-weight: 600; margin-bottom: 0.5rem; color: #374151; font-size: 0.9rem; }
.req { color: #ef4444; margin-left: 2px; }

.form-group input {
  padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 6px;
  font-size: 1rem; color: #111; background-color: #fff; transition: border 0.2s;
}
.form-group input:focus { border-color: #3b82f6; outline: none; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }

/* Buttons */
.buttons-row { display: flex; justify-content: flex-end; gap: 1rem; padding-top: 1rem; border-top: 1px solid #f3f4f6; }
.btn-continuar {
  padding: 0.75rem 1.5rem; cursor: pointer; border: none; border-radius: 6px;
  font-weight: 600; background-color: #2563eb; color: white;
  font-size: 1rem; transition: background 0.2s;
}
.btn-continuar:hover:not(:disabled) { background-color: #1d4ed8; }
.btn-continuar:disabled { background-color: #93c5fd; cursor: not-allowed; }

.btn-voltar {
  padding: 0.75rem 1.5rem; cursor: pointer; border: 1px solid #d1d5db;
  border-radius: 6px; font-weight: 600; background-color: #fff; color: #4b5563;
  font-size: 1rem; transition: background 0.2s;
}
.btn-voltar:hover { background-color: #f9fafb; border-color: #9ca3af; }

/* Responsividade */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .form-grid { grid-template-columns: 1fr; gap: 1rem; }
  .buttons-row { flex-direction: column-reverse; }
  .btn-continuar, .btn-voltar { width: 100%; }
}
</style>