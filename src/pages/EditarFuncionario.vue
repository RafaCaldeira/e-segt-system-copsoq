<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router'; 
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { FuncionarioCreateDto } from '../types/funcionario.types';
// 1. IMPORTAR O FOOTER
import AppFooter from '../components/AppFooter.vue';

const router = useRouter();
const route = useRoute(); 
const userStore = useUserStore();

const isLoading = ref(true);
const isSaving = ref(false);
const funcionarioId = parseInt(route.params.id as string); 

// Estado do Formulário
const form = ref<FuncionarioCreateDto>({
  nome: '',
  email: '',
  cpf: '',
  telefone: '',
  cargo: '',
  setor: ''
});

// Sidebar
function handleLogout() { userStore.logout(); router.push('/login'); }
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole);

// Carregar dados ao abrir a página
onMounted(async () => {
  if (!funcionarioId) {
    alert("ID inválido.");
    router.push('/funcionario');
    return;
  }

  const funcionario = await apiService.getFuncionarioById(funcionarioId);
  
  if (funcionario) {
    form.value = {
      nome: funcionario.nome,
      email: funcionario.email,
      cpf: funcionario.cpf ?? "",
      telefone: funcionario.telefone,
      cargo: funcionario.cargo,
      setor: funcionario.setor
    };
  } else {
    alert("Funcionário não encontrado.");
    router.push('/funcionario');
  }
  isLoading.value = false;
});

// Ação de Salvar (Editar)
async function salvarAlteracoes() {
  if (!form.value.nome || !form.value.email || !form.value.cpf) {
    alert("Preencha os campos obrigatórios: Nome, Email e CPF.");
    return;
  }

  isSaving.value = true;
  const sucesso = await apiService.updateFuncionario(funcionarioId, form.value);

  if (sucesso) {
    alert("Funcionário atualizado com sucesso!");
    router.push('/funcionario'); 
  } else {
    alert("Erro ao atualizar. Verifique os dados.");
  }
  isSaving.value = false;
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
        <li v-if="userStore.isCliente" class="active">
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
          <h1 class="content-title">Editar Funcionário</h1>
          
          <div v-if="isLoading" class="loading">Carregando dados...</div>

          <form v-else @submit.prevent="salvarAlteracoes" class="form-content">
            
            <div class="form-grid">
              <div class="form-group">
                <label>Nome Completo *</label>
                <input v-model="form.nome" type="text" required />
              </div>

              <div class="form-group">
                <label>Email *</label>
                <input v-model="form.email" type="email" required />
              </div>

              <div class="form-group">
                <label>CPF *</label>
                <input v-model="form.cpf" type="text" maxlength="14" required />
              </div>

              <div class="form-group">
                <label>Telefone</label>
                <input v-model="form.telefone" type="text" />
              </div>

              <div class="form-group">
                <label>Cargo</label>
                <input v-model="form.cargo" type="text" />
              </div>

              <div class="form-group">
                <label>Setor</label>
                <input v-model="form.setor" type="text" />
              </div>
            </div>

            <div class="buttons-row">
              <button type="button" class="btn-voltar" @click="voltar">Cancelar</button>
              <button type="submit" class="btn-continuar" :disabled="isSaving">
                {{ isSaving ? 'Salvando...' : 'Salvar Alterações' }}
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
  max-width: 800px; 
  width: 100%; 
  padding: 2.5rem 3rem; 
  border-radius: 12px; 
  background-color: #ffffff; 
  color: #333; 
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); 
  margin-bottom: 2rem;
}

.content-title { font-size: 1.8rem; color: #111; border-bottom: 2px solid #f3f4f6; padding-bottom: 1rem; margin-bottom: 2rem; }
.loading { text-align: center; padding: 3rem; font-size: 1.1rem; color: #6b7280; }

/* Form Styles */
.form-content { margin-top: 1rem; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 2rem; }
.form-group { display: flex; flex-direction: column; }
.form-group label { font-weight: 600; margin-bottom: 0.5rem; color: #374151; font-size: 0.9rem; }
.form-group input { padding: 0.8rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 1rem; color: #333; background-color: #fff; transition: border 0.2s; }
.form-group input:focus { border-color: #2563eb; outline: none; box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1); }

.buttons-row { display: flex; justify-content: flex-end; gap: 1rem; border-top: 1px solid #f3f4f6; padding-top: 1.5rem; }
.btn-continuar { padding: 0.8rem 1.5rem; cursor: pointer; border: none; border-radius: 6px; font-weight: 600; background-color: #2563eb; color: white; font-size: 1rem; transition: background 0.2s; }
.btn-continuar:hover { background-color: #1d4ed8; }
.btn-continuar:disabled { background-color: #93c5fd; cursor: not-allowed; }

.btn-voltar { padding: 0.8rem 1.5rem; cursor: pointer; border: 1px solid #d1d5db; border-radius: 6px; font-weight: 600; background-color: #fff; color: #4b5563; font-size: 1rem; transition: background 0.2s; }
.btn-voltar:hover { background-color: #f9fafb; color: #1f2937; }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .form-grid { grid-template-columns: 1fr; }
  .responder-container { padding: 1.5rem; }
}
</style>