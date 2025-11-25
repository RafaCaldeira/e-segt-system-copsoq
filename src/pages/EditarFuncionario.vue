<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router'; // useRoute para pegar o ID
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { FuncionarioCreateDto } from '../types/funcionario.types';

const router = useRouter();
const route = useRoute(); // Acesso à URL
const userStore = useUserStore();

const isLoading = ref(true);
const isSaving = ref(false);
const funcionarioId = parseInt(route.params.id as string); // Pega o ID da URL

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
    router.push('/funcionarios');
    return;
  }

  const funcionario = await apiService.getFuncionarioById(funcionarioId);
  
  if (funcionario) {
    // Preenche o formulário com os dados vindos da API
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
    router.push('/funcionarios');
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
  
  // Chama o método UPDATE
  const sucesso = await apiService.updateFuncionario(funcionarioId, form.value);

  if (sucesso) {
    alert("Funcionário atualizado com sucesso!");
    router.push('/funcionario'); // Volta para a lista
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
    <!-- Sidebar -->
    <nav class="sidebar">
      <img src="../assets/logo-e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon"></span> {{ displayName }}</li>
        <li class="active"><router-link to="/funcionarios"><span class="icon"></span> Editar Cadastro</router-link></li>
        <li><router-link to="/plano-de-acao"><span class="icon"></span> Plano de ação</router-link></li>
        <li><router-link to="/relatorio"><span class="icon"></span> Relatórios</router-link></li>
        <li><a href="#"><span class="icon"></span> Baixar Roadmap</a></li>
        <li><a href="#"><span class="icon"></span> Histórico</a></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon icon-logout"></span> Sair</a></li>
      </ul>
    </nav>

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
              {{ isSaving ? 'A guardar...' : 'Salvar Alterações' }}
            </button>
          </div>

        </form>
      </div>
    </main>
  </div>
</template>

<style scoped>
/* (Use os mesmos estilos do NovoFuncionario.vue para consistência) */
:global(body) { margin: 0; background-color: #f0f2f5; font-family: Arial, sans-serif; }
.app-layout { display: flex; min-height: 100vh; }
.sidebar { width: 280px; flex-shrink: 0; background-color: #ffffff; padding: 2rem 1.5rem; border-right: 1px solid #e0e0e0; }
.sidebar-logo { width: 150px; margin-bottom: 2rem; display: block; margin: 0 auto 2rem auto; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; }
.sidebar-nav li { margin-bottom: 0.5rem; }
.sidebar-nav li.user-display { font-weight: bold; padding: 1rem; border-bottom: 1px solid #eee; display: flex; align-items: center; color: #333; }
.sidebar-nav a { display: flex; align-items: center; padding: 0.8rem 1rem; border-radius: 6px; text-decoration: none; color: #555; transition: background 0.2s; }
.sidebar-nav a:hover { background-color: #f0f2f5; }
.sidebar-nav li.active a { background-color: #e0eafc; color: #3b82f6; font-weight: bold; }
.sidebar-nav .icon { width: 20px; height: 20px; margin-right: 0.8rem; background-color: #ccc; border-radius: 50%; }
.logout-item { margin-top: 2rem; }
.logout-item a { color: #d9534f; font-weight: bold; }

.main-content { flex: 1; background-color: #333; padding: 2rem; display: flex; justify-content: center; align-items: flex-start; overflow-y: auto; }
.responder-container { max-width: 800px; width: 100%; padding: 2.5rem 3rem; border-radius: 8px; background-color: #f4f7f6; color: #333; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.content-title { font-size: 2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 2rem; display: inline-block; }
.loading { text-align: center; padding: 3rem; font-size: 1.2rem; color: #666; }

.form-content { margin-top: 1rem; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 2rem; }
.form-group { display: flex; flex-direction: column; }
.form-group label { font-weight: bold; margin-bottom: 0.5rem; color: #555; font-size: 0.9rem; }
.form-group input { padding: 0.8rem; border: 1px solid #ccc; border-radius: 4px; font-size: 1rem; color: #333; background-color: #fff; }
.form-group input:focus { border-color: #3b82f6; outline: none; }

.buttons-row { display: flex; justify-content: flex-end; gap: 1rem; }
.btn-continuar { padding: 0.8rem 1.5rem; cursor: pointer; border: none; border-radius: 6px; font-weight: bold; background-color: #3b82f6; color: white; font-size: 1rem; transition: background 0.2s; }
.btn-continuar:hover { background-color: #2563eb; }
.btn-voltar { padding: 0.8rem 1.5rem; cursor: pointer; border: 1px solid #ccc; border-radius: 6px; font-weight: bold; background-color: #fff; color: #555; font-size: 1rem; transition: background 0.2s; }
.btn-voltar:hover { background-color: #e9ecef; }
</style>