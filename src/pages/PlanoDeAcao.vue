<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { PlanoDeAcao, Acao } from '../types/plano.types';
import { useRouter } from 'vue-router';

// --- Estado ---
const planos = ref<PlanoDeAcao[]>([]);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

// Estado para Nova Ação
const novaAcaoTexto = ref('');
const isAddingAcao = ref(false);

const userStore = useUserStore();
const router = useRouter();

// --- Permissões ---
const podeEditar = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');

// --- Lógica da Sidebar (Copiada para manter layout) ---
function handleLogout() { userStore.logout(); router.push('/login'); }
const displayName = computed(() => {
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  return userStore.nomeEmpresa || "Cliente";
});

// --- Lógica do Plano ---
const planoAtual = computed(() => planos.value.length > 0 ? planos.value[0] : null);

// Progresso (Cálculo da barra)
const progresso = computed(() => {
  if (!planoAtual.value || planoAtual.value.acoes.length === 0) return 0;
  const concluidas = planoAtual.value.acoes.filter(a => a.status === 'Concluido').length;
  return Math.round((concluidas / planoAtual.value.acoes.length) * 100);
});

const acoesConcluidasCount = computed(() => 
  planoAtual.value ? planoAtual.value.acoes.filter(a => a.status === 'Concluido').length : 0
);

onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }
  await carregarPlanos();
});

async function carregarPlanos() {
  isLoading.value = true;
  let empresaId = userStore.empresaId;
  if (userStore.isAdmin && !empresaId) empresaId = 3; // ID de Teste para Admin

  if (empresaId) {
    const data = await apiService.getPlanosPorEmpresa(empresaId);
    if (data) planos.value = data;
  }
  isLoading.value = false;
}

async function criarPlanoInicial() {
  if (!podeEditar.value) return;
  let empresaId = userStore.empresaId || 3; 
  
  const sucesso = await apiService.createPlano({
    titulo: 'Plano de Ação Inicial',
    descricao: 'Plano gerado a partir da avaliação COPSOQ.',
    empresaID: empresaId
  });
  if (sucesso) await carregarPlanos();
}

async function adicionarAcao() {
  if (!novaAcaoTexto.value || !planoAtual.value) return;
  isAddingAcao.value = true;
  
  const novaAcao = await apiService.addAcao(planoAtual.value.id, {
    descricao: novaAcaoTexto.value
  });

  if (novaAcao) {
    planoAtual.value.acoes.push(novaAcao);
    novaAcaoTexto.value = '';
  }
  isAddingAcao.value = false;
}

async function toggleStatus(acao: Acao) {
  if (!podeEditar.value) return; // Clientes apenas visualizam (opcional)
  
  const novoStatus = acao.status === 'Concluido' ? 'Pendente' : 'Concluido';
  // Atualiza na UI imediatamente (otimista)
  const statusAntigo = acao.status;
  acao.status = novoStatus; 

  const sucesso = await apiService.updateStatusAcao(acao.id, novoStatus);
  if (!sucesso) acao.status = statusAntigo; // Reverte se falhar
}
</script>

<template>
  <div class="app-layout">
    <nav class="sidebar">
      <img src="../assets/logo-e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon"></span> {{ displayName }}</li>
        <li><a href="#"><span class="icon"></span> Editar Cadastro</a></li>
        <li class="active"><router-link to="/plano-de-acao"><span class="icon"></span> Plano de ação</router-link></li>
        <li><router-link to="/relatorio"><span class="icon"></span> Relatórios</router-link></li>
        <li><a href="#"><span class="icon"></span> Baixar Roadmap</a></li>
        <li><a href="#"><span class="icon"></span> Histórico</a></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon icon-logout"></span> Sair</a></li>
      </ul>
    </nav>

    <main class="main-content">
      <div class="responder-container">
        
        <h1 class="content-title">Plano de Ação</h1>

        <div v-if="isLoading" class="loading">A carregar planos...</div>

        <div v-else>
          <!-- Caso 1: Não existe plano -->
          <div v-if="!planoAtual" class="empty-state">
            <p>Esta empresa ainda não possui um Plano de Ação definido.</p>
            <button v-if="podeEditar" @click="criarPlanoInicial" class="btn-continuar">
              Criar Plano Agora
            </button>
          </div>

          <!-- Caso 2: Plano existe -->
          <div v-else>
            <h3>{{ planoAtual.titulo }}</h3>
            <p class="desc">{{ planoAtual.descricao }}</p>

            <!-- Barra de Progresso -->
            <div class="progress-section">
              <div class="progress-labels">
                <span>Progresso</span>
                <span>{{ acoesConcluidasCount }}/{{ planoAtual.acoes.length }} ações concluídas - {{ progresso }}%</span>
              </div>
              <div class="progress-bar-bg">
                <div class="progress-bar-fill" :style="{ width: progresso + '%' }"></div>
              </div>
            </div>

            <!-- Lista de Ações -->
            <div class="actions-list">
              <div v-for="acao in planoAtual.acoes" :key="acao.id" class="action-item">
                <div class="action-text">
                  • {{ acao.descricao }}
                </div>
                <div class="action-status">
                  <button 
                    @click="toggleStatus(acao)"
                    class="status-badge" 
                    :class="acao.status === 'Concluido' ? 'status-concluido' : 'status-pendente'"
                    :disabled="!podeEditar"
                  >
                    {{ acao.status === 'Concluido' ? 'Concluído' : 'Pendente' }}
                  </button>
                </div>
              </div>
            </div>

            <!-- Adicionar Nova Ação (Só Admin/Psicologo) -->
            <div v-if="podeEditar" class="add-action-form">
              <input 
                v-model="novaAcaoTexto" 
                placeholder="Descreva a nova ação de melhoria..." 
                @keyup.enter="adicionarAcao"
              />
              <button @click="adicionarAcao" :disabled="isAddingAcao || !novaAcaoTexto">
                Adicionar
              </button>
            </div>

          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* Reutilizando estilos base */
:global(body) { margin: 0; background-color: #f0f2f5; font-family: Arial, sans-serif; }
.app-layout { display: flex; min-height: 100vh; }
.sidebar { width: 280px; flex-shrink: 0; background-color: #ffffff; padding: 2rem 1.5rem; border-right: 1px solid #e0e0e0; }
.sidebar-logo { width: 150px; margin-bottom: 2.5rem; display: block; margin-left: auto; margin-right: auto; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; }
.sidebar-nav li { margin-bottom: 0.5rem; }
.sidebar-nav li.user-display { font-size: 1.2rem; font-weight: bold; color: #333; padding: 1rem; margin-bottom: 1.5rem; border-bottom: 1px solid #eee; display: flex; align-items: center; }
.sidebar-nav a, .sidebar-nav :deep(a) { display: flex; align-items: center; padding: 0.8rem 1rem; border-radius: 6px; text-decoration: none; color: #555; font-weight: 500; transition: background-color 0.2s; cursor: pointer; }
.sidebar-nav a:hover, .sidebar-nav :deep(a:hover) { background-color: #f0f2f5; }
.sidebar-nav li.active a, .sidebar-nav li.active :deep(a) { background-color: #e0eafc; color: #3b82f6; font-weight: bold; }
.sidebar-nav .icon { display: inline-block; width: 20px; height: 20px; margin-right: 0.8rem; background-color: #ccc; border-radius: 50%; flex-shrink: 0; }
.sidebar-nav li.logout-item { margin-top: 2rem; }
.sidebar-nav li.logout-item a { color: #d9534f; font-weight: bold; }

.main-content { flex: 1; background-color: #333; padding: 2rem; display: flex; justify-content: center; align-items: flex-start; overflow-y: auto; }
.responder-container { max-width: 900px; width: 100%; padding: 2.5rem 3rem; border-radius: 8px; background-color: #f4f7f6; color: #333; }
.loading { text-align: center; padding: 3rem; }
.content-title { font-size: 2.2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 1.5rem; display: inline-block; }
.btn-continuar { padding: 0.8rem 1.5rem; cursor: pointer; border: none; border-radius: 6px; font-weight: bold; background-color: #3b82f6; color: white; }

/* Estilos Específicos do Plano de Ação */
.desc { color: #666; margin-bottom: 2rem; }

.progress-section { background-color: #e9ecef; padding: 1.5rem; border-radius: 8px; margin-bottom: 2rem; }
.progress-labels { display: flex; justify-content: space-between; margin-bottom: 0.5rem; font-weight: bold; color: #555; }
.progress-bar-bg { width: 100%; height: 12px; background-color: #ccc; border-radius: 6px; overflow: hidden; }
.progress-bar-fill { height: 100%; background-color: #28a745; transition: width 0.5s ease; }

.actions-list { margin-bottom: 2rem; }
.action-item { display: flex; justify-content: space-between; align-items: center; padding: 1rem; border-bottom: 1px solid #ddd; background-color: #fff; margin-bottom: 0.5rem; border-radius: 4px; }
.action-text { font-size: 1.1rem; }

.status-badge { padding: 0.4rem 0.8rem; border-radius: 4px; border: none; color: white; font-weight: bold; cursor: pointer; font-size: 0.9rem; }
.status-pendente { background-color: #483D8B; /* Roxo do seu esboço */ }
.status-concluido { background-color: #28a745; /* Verde */ }
.status-badge:disabled { cursor: default; opacity: 1; }

.add-action-form { display: flex; gap: 1rem; margin-top: 2rem; padding-top: 1rem; border-top: 2px dashed #ccc; }
.add-action-form input { flex: 1; padding: 0.8rem; border: 1px solid #ccc; border-radius: 4px; }
.add-action-form button { padding: 0.8rem 1.5rem; background-color: #3b82f6; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold; }
</style>