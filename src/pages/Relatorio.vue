<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import { pdfService } from '../services/pdf.service'; // <--- 1. O IMPORT FICA AQUI
import type { RelatorioCompletoDto } from '../types/relatorio.types';
import { useRouter } from 'vue-router';

// Estado
const relatorio = ref<RelatorioCompletoDto | null>(null);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- Lógica da Sidebar ---
function handleLogout() {
  userStore.logout();
  router.push('/login');
}

const displayName = computed(() => {
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  if (userStore.isCliente && userStore.nomeEmpresa) {
    return userStore.nomeEmpresa;
  }
  return userStore.userRole; 
});

// --- Lógica do Relatório ---
onMounted(async () => {
  if (!userStore.isLoggedIn) {
    router.push('/login');
    return;
  }

  // Apenas 'Cliente' ou 'Admin'/'Psicologo' podem ver
  if (!userStore.isCliente && !userStore.isAdmin && userStore.userRole !== 'Psicologo') {
     errorMessage.value = "Você não tem permissão para ver esta página.";
     isLoading.value = false;
     return;
  }

  // === BUSCAR OS DADOS ===
  const questionarioIdParaBuscar = 6; 
  let empresaIdParaBuscar = userStore.empresaId;

  if (userStore.isAdmin && !empresaIdParaBuscar) {
    empresaIdParaBuscar = 3; // O ID da "Empresa de Teste SA"
  }

  if (!empresaIdParaBuscar) {
    errorMessage.value = "ID da empresa não encontrado. Não é possível carregar relatórios.";
    isLoading.value = false;
    return;
  }

  isLoading.value = true;
  const data = await apiService.getRelatorio(empresaIdParaBuscar, questionarioIdParaBuscar);

  if (data) {
    relatorio.value = data;
  } else {
    errorMessage.value = "Não foi possível carregar o relatório. (Provavelmente ainda não há respostas)";
  }
  isLoading.value = false;
});

// Função auxiliar para o estilo do "Nível de Risco" na tela
function getRiscoClass(nivelRisco: string): string {
  if (!nivelRisco) return 'risco-desconhecido';
  const risco = nivelRisco.toLowerCase();
  
  if (risco.includes('alto')) return 'risco-alto';
  if (risco.includes('médio') || risco.includes('moderado')) return 'risco-medio';
  if (risco.includes('baixo') || risco.includes('saudável')) return 'risco-baixo';
  
  return 'risco-desconhecido';
}

// <--- 2. A FUNÇÃO FICA AQUI (No final do script)
function downloadPDF() {
  if (relatorio.value) {
    pdfService.gerarRelatorioPDF(relatorio.value);
  }
}
</script>

<template>
  <div class="app-layout">
    
    <!-- 1. BARRA LATERAL (Sidebar) -->
    <nav class="sidebar">
      <img src="../assets/logo-e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      
      <ul class="sidebar-nav">
        <li class="user-display">
          <span class="icon"></span> {{ displayName }}
        </li>
        <li><a href="#"><span class="icon"></span> Editar Cadastro</a></li>
        <li><a href="#"><span class="icon"></span> Plano de ação</a></li>
        <li class="active"><router-link to="/relatorio"><span class="icon"></span> Relatórios</router-link></li> 
        <li><a href="#"><span class="icon"></span> Baixar Roadmap</a></li>
        <li><a href="#"><span class="icon"></span> Histórico</a></li>
        <li class="logout-item">
          <a @click="handleLogout" href="#">
            <span class="icon icon-logout"></span> Sair
          </a>
        </li>
      </ul>
    </nav>

    <!-- 2. CONTEÚDO PRINCIPAL (Relatório) -->
    <main class="main-content">
      <div class="responder-container">
        
        <div v-if="isLoading" class="loading">
          A calcular relatório...
        </div>
        <div v-else-if="errorMessage" class="error-message">
          {{ errorMessage }}
        </div>

        <div v-else-if="relatorio">
          <h1 class="content-title">Relatórios</h1>
          
          <div class="filters-container">
            <div class="filter-item">
              <label for="filtro-data">Buscar</label>
              <input type="date" id="filtro-data" value="2025-11-17">
            </div>
            <div class="filter-item">
              <label for="filtro-questionario">Questionário</label>
              <select id="filtro-questionario">
                <option>{{ relatorio.tituloQuestionario }} ({{ relatorio.totalRespondentes }} respondentes)</option>
              </select>
            </div>
          </div>
          
          <h2>Painel de indicadores Gerais</h2>

          <table class="report-table">
            <thead>
              <tr>
                <th>Indicador</th>
                <th>Resultados</th>
                <th>Nível de risco</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in relatorio.resultados" :key="item.nomeIndicador">
                <td>{{ item.nomeIndicador }}</td>
                <td><strong>{{ item.scorePercentual.toFixed(1) }}%</strong></td> 
                <td>
                  <span class="risk-dot" :class="getRiscoClass(item.nivelRisco)"></span>
                  {{ item.nivelRisco }}
                </td>
              </tr>
            </tbody>
          </table>

          <!-- <--- 3. O BOTÃO FICA AQUI (Dentro da div navegacao) -->
          <div class="navegacao">
            <span></span> 
            <button class="btn-continuar" @click="downloadPDF">Baixar prévia</button>
          </div>

        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
/* (Mantenha o seu CSS existente, está perfeito) */
:global(body) { margin: 0; background-color: #f0f2f5; }
.app-layout { display: flex; min-height: 100vh; font-family: Arial, sans-serif; }
.sidebar { width: 280px; flex-shrink: 0; background-color: #ffffff; padding: 2rem 1.5rem; border-right: 1px solid #e0e0e0; }
.sidebar-logo { width: 150px; margin-bottom: 2.5rem; display: block; margin-left: auto; margin-right: auto; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; }
.sidebar-nav li { margin-bottom: 0.5rem; }
.sidebar-nav li.user-display { font-size: 1.2rem; font-weight: bold; color: #333; padding: 1rem; margin-bottom: 1.5rem; border-bottom: 1px solid #eee; display: flex; align-items: center; }
.sidebar-nav a, .sidebar-nav :deep(a) { display: flex; align-items: center; padding: 0.8rem 1rem; border-radius: 6px; text-decoration: none; color: #555; font-weight: 500; transition: background-color 0.2s, color 0.2s; cursor: pointer; }
.sidebar-nav a:hover, .sidebar-nav :deep(a:hover) { background-color: #f0f2f5; }
.sidebar-nav li.active a, .sidebar-nav li.active :deep(a) { background-color: #e0eafc; color: #3b82f6; font-weight: bold; }
.sidebar-nav .icon { display: inline-block; width: 20px; height: 20px; margin-right: 0.8rem; background-color: #ccc; border-radius: 50%; flex-shrink: 0; }
.sidebar-nav li.logout-item { margin-top: 2rem; }
.sidebar-nav li.logout-item a { color: #d9534f; font-weight: bold; }
.sidebar-nav li.logout-item a:hover { background-color: #fdf2f2; }
.main-content { flex: 1; background-color: #333; padding: 2rem; display: flex; justify-content: center; align-items: flex-start; overflow-y: auto; }
.responder-container { max-width: 900px; width: 100%; margin: 0; padding: 2.5rem 3rem; border-radius: 8px; background-color: #f4f7f6; color: #333; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.loading, .error-message, .no-data { text-align: center; padding: 3rem; font-size: 1.2rem; color: #555; }
.error-message { color: #d9534f; }
h1.content-title { font-size: 2.2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 2rem; display: inline-block; }
h2 { font-size: 1.5rem; margin-bottom: 1.5rem; color: #444; font-weight: bold; }
.filters-container { display: flex; gap: 1.5rem; margin-bottom: 2rem; padding-bottom: 1.5rem; border-bottom: 1px solid #ddd; }
.filter-item { display: flex; flex-direction: column; }
.filter-item label { font-size: 0.9rem; color: #666; margin-bottom: 0.3rem; font-weight: bold; }
/* A sua correção CSS para o input date */
.filter-item input, .filter-item select { 
  padding: 0.6rem; border: 1px solid #ccc; border-radius: 4px; background-color: #fff; font-size: 1rem; font-family: Arial, sans-serif; box-sizing: border-box; 
  color: #333 !important; min-width: 160px; height: 42px; opacity: 1 !important;
}
.filter-item input[type="date"] { appearance: none; -webkit-appearance: none; display: inline-block; position: relative; }
.filter-item input[type="date"]::-webkit-calendar-picker-indicator { opacity: 1; display: block; cursor: pointer; background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="16" height="15" viewBox="0 0 24 24"><path fill="%23333" d="M20 3h-1V1h-2v2H7V1H5v2H4c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 18H4V8h16v13z"/></svg>'); }

.report-table { width: 100%; border-collapse: collapse; margin-top: 1rem; color: #333; }
.report-table th, .report-table td { border-bottom: 1px solid #ddd; padding: 1rem; text-align: left; }
.report-table th { background-color: #eee; font-size: 0.9rem; text-transform: uppercase; color: #666; }
.report-table td { font-size: 1.1rem; }
.report-table td:nth-child(2) { font-weight: bold; font-size: 1.2rem; }
.risk-dot { display: inline-block; width: 12px; height: 12px; border-radius: 50%; margin-right: 0.5rem; }
.risco-alto { background-color: #d9534f; }
.risco-medio { background-color: #f0ad4e; }
.risco-baixo { background-color: #5cb85c; }
.risco-desconhecido { background-color: #ccc; }
.navegacao { margin-top: 2.5rem; display: flex; justify-content: flex-end; align-items: center; }
.btn-continuar { padding: 0.8rem 1.5rem; cursor: pointer; border: none; border-radius: 6px; font-weight: bold; font-size: 1rem; transition: background-color 0.2s; background-color: #3b82f6; color: white; }
.btn-continuar:hover { opacity: 0.8; }
</style>