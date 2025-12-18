<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { FuncionarioCreateDto } from '../types/funcionario.types';
// 1. IMPORTAR O FOOTER
import AppFooter from '../components/AppFooter.vue';

const router = useRouter();
const userStore = useUserStore();

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
function handleLogout() { userStore.logout(); router.push('/login'); }
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole);

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
    
    <nav class="sidebar">
      <div class="logo-area">
        <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      </div>
      
      <div class="user-badge">{{ displayName }}</div>

      <ul class="sidebar-nav">
        <li v-if="userStore.isAdmin">
          <router-link to="/criar-questionario"><span class="icon">📝</span> Criar Questionário</router-link>
        </li>
        <li v-if="userStore.isAdmin">
          <router-link to="/disparo"><span class="icon">📨</span> Enviar Questionário</router-link>
        </li>

        <li v-if="userStore.isCliente">
            <router-link to="/editar-cadastro"><span class="icon">⚙️</span> Editar Cadastro</router-link>
        </li>
        <li v-if="userStore.isCliente">
            <router-link to="/funcionario"><span class="icon">👥</span> Funcionários</router-link>
        </li>

        <li v-if="userStore.userRole === 'Psicologo'">
            <router-link to="/psicologo"><span class="icon">🧠</span> Área do Psicólogo</router-link>
        </li>

        <li><router-link to="/plano-de-acao"><span class="icon">📋</span> Plano de Ação</router-link></li>
        <li><router-link to="/relatorio"><span class="icon">📊</span> Relatórios</router-link></li>
        <li><router-link to="/historico"><span class="icon">📜</span> Histórico</router-link></li>
        
        <li class="logout-item"><a @click.prevent="handleLogout" href="#"><span class="icon">🚪</span> Sair</a></li>
      </ul>
    </nav>

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

/* Sidebar */
.sidebar { 
  width: 260px; 
  background-color: #ffffff; 
  border-right: 1px solid #e5e7eb; 
  display: flex; 
  flex-direction: column; 
  padding: 1.5rem 1rem; 
  flex-shrink: 0; 
  z-index: 10;
}
.sidebar-logo { width: 120px; display: block; margin: 0 auto 1.5rem auto; }
.user-badge { background: #f3f4f6; padding: 0.5rem; border-radius: 6px; text-align: center; font-weight: bold; margin-bottom: 1.5rem; color: #374151; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; flex: 1; overflow-y: auto; }
.sidebar-nav li { margin-bottom: 5px; }
.sidebar-nav a { display: flex; align-items: center; padding: 0.75rem 1rem; color: #4b5563; text-decoration: none; border-radius: 6px; font-weight: 500; transition: all 0.2s; }
.sidebar-nav a:hover { background: #f3f4f6; color: #111; }
.sidebar-nav li.active a { background: #eff6ff; color: #2563eb; font-weight: 600; }
.sidebar-nav .icon { margin-right: 10px; min-width: 20px; text-align: center; }
.logout-item { margin-top: auto; border-top: 1px solid #f3f4f6; padding-top: 1rem; }
.logout-item a { color: #ef4444; }

/* --- MAIN WRAPPER (Novo container flex column) --- */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh; /* Altura total da viewport */
  overflow-y: auto; /* Scroll acontece aqui */
}

/* --- MAIN CONTENT --- */
.main-content { 
  flex: 1; /* Empurra o footer para baixo */
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
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .form-grid { grid-template-columns: 1fr; gap: 1rem; }
  .buttons-row { flex-direction: column-reverse; }
  .btn-continuar, .btn-voltar { width: 100%; }
}
</style>