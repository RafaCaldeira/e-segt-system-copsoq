<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
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
  return userStore.userRole; // Fallback
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
  
  // TODO: Tornar o 'questionarioId' dinâmico com o <select>
  const questionarioIdParaBuscar = 6; 
  let empresaIdParaBuscar = userStore.empresaId;

  // Se for Admin/Psicologo, eles podem precisar de um seletor de empresa.
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

// Função auxiliar para o estilo do "Nível de Risco"
function getRiscoClass(nivelRisco: string): string {
  if (!nivelRisco) return 'risco-desconhecido';
  const risco = nivelRisco.toLowerCase();
  
  if (risco.includes('alto')) return 'risco-alto';
  if (risco.includes('médio') || risco.includes('moderado')) return 'risco-medio';
  if (risco.includes('baixo') || risco.includes('saudável')) return 'risco-baixo';
  
  return 'risco-desconhecido';
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
        <!-- Marca 'Relatórios' como ativo -->
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
      <div class="responder-container"> <!-- Reutilizando o "card" -->
        
        <div v-if="isLoading" class="loading">
          A calcular relatório...
        </div>
        <div v-else-if="errorMessage" class="error-message">
          {{ errorMessage }}
        </div>

        <!-- O Relatório (Baseado no esboço ...153229.png) -->
        <div v-else-if="relatorio">
          <h1 class="content-title">Relatórios</h1>
          
          <!-- Filtros (Placeholders) -->
          <div class="filters-container">
            <div class="filter-item">
              <label for="filtro-data">Buscar</label>
              <!-- (O seu esboço mostrava um ícone de calendário,
                   o <input type="date"> faz isso automaticamente) -->
              <input type="date" id="filtro-data" value="2025-11-17">
            </div>
            <div class="filter-item">
              <label for="filtro-questionario">Questionário</label>
              <select id="filtro-questionario">
                <!-- (Isto já está a puxar o nome do questionário que carregámos) -->
                <option>{{ relatorio.tituloQuestionario }} ({{ relatorio.totalRespondentes }} respondentes)</option>
                <!-- (Mais tarde, podemos carregar todos os questionários aqui) -->
              </select>
            </div>
          </div>
          
          <h2>Painel de indicadores Gerais</h2>

          <!-- Tabela de Resultados -->
          <table class="report-table">
            <thead>
              <tr>
                <th>Indicador</th>
                <th>Resultados</th>
                <th>Nível de risco</th>
              </tr>
            </thead>
            <tbody>
              <!-- Loop v-for nos resultados calculados -->
              <tr v-for="item in relatorio.resultados" :key="item.nomeIndicador">
                <td>{{ item.nomeIndicador }}</td>
                <!-- Mostra a % (como no esboço) -->
                <td><strong>{{ item.scorePercentual.toFixed(1) }}%</strong></td> 
                <td>
                  <!-- Bolinha colorida + Texto -->
                  <span classOwes="risk-dot" :class="getRiscoClass(item.nivelRisco)"></span>
                  {{ item.nivelRisco }}
                </td>
              </tr>
            </tbody>
          </table>

          <div class="navegacao">
            <span></span> <!-- Espaçador -->
            <button class="btn-continuar">Baixar prévia</button>
          </div>

        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>

:global(body) {
  margin: 0;
  background-color: #f0f2f5; 
}
.app-layout {
  display: flex;
  min-height: 100vh;
  font-family: Arial, sans-serif;
}
.sidebar {
  width: 280px;
  flex-shrink: 0;
  background-color: #ffffff;
  padding: 2rem 1.5rem;
  border-right: 1px solid #e0e0e0;
}
.sidebar-logo {
  width: 150px;
  margin-bottom: 2.5rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}
.sidebar-nav {
  list-style: none;
  padding: 0;
  margin: 0;
}
.sidebar-nav li {
  margin-bottom: 0.5rem;
}
.sidebar-nav li.user-display {
  font-size: 1.2rem;
  font-weight: bold;
  color: #333;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  align-items: center;
}
/* Estilo para router-link e a */
.sidebar-nav a, .sidebar-nav :deep(a) { /* :deep(a) para estilizar o <router-link> */
  display: flex;
  align-items: center;
  padding: 0.8rem 1rem;
  border-radius: 6px;
  text-decoration: none;
  color: #555;
  font-weight: 500;
  transition: background-color 0.2s, color 0.2s;
  cursor: pointer;
}
.sidebar-nav a:hover, .sidebar-nav :deep(a:hover) {
  background-color: #f0f2f5;
}
.sidebar-nav li.active a, .sidebar-nav li.active :deep(a) {
  background-color: #e0eafc; 
  color: #3b82f6; 
  font-weight: bold;
}
.sidebar-nav .icon {
  display: inline-block;
  width: 20px;
  height: 20px;
  margin-right: 0.8rem;
  background-color: #ccc; 
  border-radius: 50%;
  flex-shrink: 0;
}
.sidebar-nav li.logout-item {
  margin-top: 2rem;
}
.sidebar-nav li.logout-item a {
  color: #d9534f;
  font-weight: bold;
}
.sidebar-nav li.logout-item a:hover {
  background-color: #fdf2f2;
}
.main-content {
  flex: 1;
  background-color: #333;
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  overflow-y: auto;
}
.responder-container {
  max-width: 900px;
  width: 100%;
  margin: 0;
  padding: 2.5rem 3rem;
  border-radius: 8px;
  background-color: #f4f7f6;
  color: #333; 
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}
.loading, .error-message, .no-data {
  text-align: center;
  padding: 3rem;
  font-size: 1.2rem;
  color: #555;
}
.error-message { color: #d9534f; }
h1.content-title {
  font-size: 2.2rem;
  color: #333;
  border-bottom: 4px solid #3b82f6; 
  padding-bottom: 0.5rem;
  margin-bottom: 2rem;
  display: inline-block;
}
h2 {
  font-size: 1.5rem;
  margin-bottom: 1.5rem;
  color: #444;
  font-weight: bold;
}

/* --- NOVOS ESTILOS PARA RELATÓRIO --- */

.filters-container {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid #ddd;
}
.filter-item {
  display: flex;
  flex-direction: column;
}
.filter-item label {
  font-size: 0.9rem;
  color: #666;
  margin-bottom: 0.3rem;
  font-weight: bold;
}
.filter-item input, .filter-item select {
  padding: 0.6rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  background-color: #fff;
  color: #333;
  min-width: 150px;
  font-family: Arial, sans-serif; /* Garante consistência da fonte */
  box-sizing: border-box;
}

.report-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
  color: #333;
}
.report-table th,
.report-table td {
  border-bottom: 1px solid #ddd;
  padding: 1rem;
  text-align: left;
}
.report-table th {
  background-color: #eee;
  font-size: 0.9rem;
  text-transform: uppercase;
  color: #666;
}
.report-table td {
  font-size: 1.1rem;
}
.report-table td:nth-child(2) {
  font-weight: bold;
  font-size: 1.2rem;
}

/* Bolinhas de Risco */
.risk-dot {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-right: 0.5rem;
}
.risco-alto { background-color: #d9534f; } /* Vermelho */
.risco-medio { background-color: #f0ad4e; } /* Amarelo */
.risco-baixo { background-color: #5cb85c; } /* Verde */
.risco-desconhecido { background-color: #ccc; }

.navegacao {
  margin-top: 2.5rem;
  display: flex;
  justify-content: flex-end; /* Alinha o botão à direita */
  align-items: center;
}
.btn-continuar { /* Reutilizando o estilo do botão */
  padding: 0.8rem 1.5rem;
  cursor: pointer;
  border: none;
  border-radius: 6px;
  font-weight: bold;
  font-size: 1rem;
  transition: background-color 0.2s;
  background-color: #3b82f6; 
  color: white;
}
.btn-continuar:hover {
  opacity: 0.8;
}
</style>