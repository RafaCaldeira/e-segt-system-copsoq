<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import { pdfService } from '../services/pdf.service';
import type { RelatorioCompletoDto } from '../types/relatorio.types';
import type { Empresa } from '../types/empresa.types';
import type { Questionario } from '../types/questionario.types'; 
import { useRouter } from 'vue-router';
// 1. IMPORTAR O FOOTER
import AppFooter from '../components/AppFooter.vue';

// --- Estado ---
const relatorio = ref<RelatorioCompletoDto | null>(null);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);

// Listas para os Selects
const empresas = ref<Empresa[]>([]);
const questionarios = ref<Questionario[]>([]);

// Seleções Atuais
const selectedEmpresaId = ref<number | null>(null);
const selectedQuestionarioId = ref<number | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- Permissões ---
const podeTrocarEmpresa = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');

const displayName = computed(() => {
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  return userStore.nomeEmpresa || "Cliente";
});

// --- Inicialização ---
onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }

  isLoading.value = true;
  try {
    // 1. Carregar Lista de Questionários Disponíveis
    const listaQuest = await apiService.getQuestionarios();
    
    if (listaQuest) {
        questionarios.value = listaQuest;
        const primeiroQuestionario = listaQuest[0];
        if (primeiroQuestionario) {
            selectedQuestionarioId.value = primeiroQuestionario.id;
        }
    }

    // 2. Carregar Empresas ou Definir a Empresa Atual
    if (podeTrocarEmpresa.value) {
      const listaEmpresas = await apiService.getEmpresas();
      if (listaEmpresas) empresas.value = listaEmpresas;
    } else {
      selectedEmpresaId.value = userStore.empresaId;
    }

  } catch (e) {
    errorMessage.value = "Erro ao carregar listas iniciais.";
  } finally {
    isLoading.value = false;
  }
});

// --- Observadores ---
watch([selectedEmpresaId, selectedQuestionarioId], async ([novoEmpresaId, novoQuestId]) => {
    if (novoEmpresaId && novoQuestId) {
        await carregarRelatorio(novoEmpresaId, novoQuestId);
    } else {
        relatorio.value = null;
    }
});

async function carregarRelatorio(empresaId: number, questId: number) {
    isLoading.value = true;
    errorMessage.value = null;
    relatorio.value = null;

    try {
        const data = await apiService.getRelatorio(empresaId, questId);
        if (data) {
            relatorio.value = data;
        } else {
            errorMessage.value = "Ainda não há respostas suficientes para gerar este relatório.";
        }
    } catch (e) {
        errorMessage.value = "Erro ao buscar dados do relatório.";
    } finally {
        isLoading.value = false;
    }
}

// --- Auxiliares Visual ---
function getRiscoClass(nivelRisco: string): string {
  if (!nivelRisco) return 'risco-desconhecido';
  const risco = nivelRisco.toLowerCase();
  if (risco.includes('alto')) return 'risco-alto';
  if (risco.includes('médio') || risco.includes('moderado')) return 'risco-medio';
  if (risco.includes('baixo') || risco.includes('saudável')) return 'risco-baixo';
  return 'risco-desconhecido';
}

function downloadPDF() {
  if (relatorio.value) pdfService.gerarRelatorioPDF(relatorio.value);
}

function handleLogout() { userStore.logout(); router.push('/login'); }
</script>

<template>
  <div class="app-layout">
    <nav class="sidebar">
      <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">

      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon">👤</span> {{ displayName }}</li>
        
        <li v-if="userStore.isAdmin"><router-link to="/criar-questionario"><span class="icon">📝</span> Criar Questionário</router-link></li>
        <li v-if="userStore.isAdmin"><router-link to="/disparo"><span class="icon">📨</span> Enviar Questionário</router-link></li>
        
        <li v-if="userStore.isCliente"><router-link to="/editar-cadastro"><span class="icon">⚙️</span> Editar Cadastro</router-link></li>
        
        <li v-if="userStore.userRole === 'Psicologo'"><router-link to="/psicologo"><span class="icon">🧠</span> Área do Psicólogo</router-link></li>
        
        <li v-if="userStore.isCliente"><router-link to="/funcionario"><span class="icon">👥</span> Funcionario</router-link></li>
        
        <li class="active"><router-link to="/relatorio"><span class="icon">📊</span> Relatórios</router-link></li> 
        <li><router-link to="/plano-de-acao"><span class="icon">📋</span> Plano de ação</router-link></li>
        <li><router-link to="/historico"><span class="icon">📜</span> Histórico</router-link></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon">🚪</span> Sair</a></li>
      </ul>
    </nav>

    <div class="main-wrapper">
      <main class="main-content">
        <div class="report-wrapper">
          
          <header class="page-header">
              <h1 class="content-title">Painel de Relatórios</h1>
              <p class="subtitle">Selecione os parâmetros abaixo para analisar os indicadores.</p>
          </header>

          <div class="filters-card fade-in">
              <div class="filter-group" v-if="podeTrocarEmpresa">
                  <label>🏢 Empresa</label>
                  <select v-model="selectedEmpresaId">
                      <option :value="null" disabled>-- Selecione a Empresa --</option>
                      <option v-for="emp in empresas" :key="emp.id" :value="emp.id">
                          {{ emp.nomeEmpresa }}
                      </option>
                  </select>
              </div>

              <div class="filter-group flex-grow">
                  <label>📝 Questionário</label>
                  <select v-model="selectedQuestionarioId">
                      <option :value="null" disabled>-- Selecione o Modelo --</option>
                      <option v-for="q in questionarios" :key="q.id" :value="q.id">
                          {{ q.titulo }}
                      </option>
                  </select>
              </div>
              
                <div class="filter-group">
                  <label>📅 Data de Referência</label>
                  <div class="fake-input">{{ new Date().toLocaleDateString('pt-BR') }}</div>
              </div>
          </div>

          <div v-if="isLoading" class="loading-state">
              <div class="spinner"></div> Carregando análise...
          </div>

          <div v-else-if="errorMessage" class="info-banner error">
              ⚠️ {{ errorMessage }}
          </div>

          <div v-else-if="!selectedEmpresaId || !selectedQuestionarioId" class="info-banner info">
              👆 Selecione uma empresa e um questionário acima para gerar o relatório.
          </div>

          <div v-else-if="relatorio" class="report-content fade-in">
              
              <div class="stats-row">
                  <div class="stat-card">
                      <span class="stat-label">Total de Respostas</span>
                      <span class="stat-value">{{ relatorio.totalRespondentes }}</span>
                  </div>
                  <div class="stat-card">
                      <span class="stat-label">Score Global</span>
                      <span class="stat-value highlight">
                          {{ (relatorio.resultados.reduce((acc, curr) => acc + curr.scorePercentual, 0) / relatorio.resultados.length).toFixed(1) }}%
                      </span>
                  </div>
              </div>

              <h3 class="section-title">Indicadores Detalhados (Dimensões)</h3>
              
              <div class="table-container">
                  <table class="report-table">
                      <thead>
                          <tr>
                              <th>Indicador</th>
                              <th class="center">Score Médio</th>
                              <th>Classificação de Risco</th>
                          </tr>
                      </thead>
                      <tbody>
                          <tr v-for="item in relatorio.resultados" :key="item.nomeIndicador">
                              <td class="fw-bold">{{ item.nomeIndicador }}</td>
                              <td class="center">
                                  <div class="score-pill" :style="{ width: item.scorePercentual + '%' }">
                                      {{ item.scorePercentual.toFixed(1) }}%
                                  </div>
                              </td>
                              <td>
                                  <span class="risk-badge" :class="getRiscoClass(item.nivelRisco)">
                                      <span class="dot"></span> {{ item.nivelRisco }}
                                  </span>
                              </td>
                          </tr>
                      </tbody>
                  </table>
              </div>

              <div class="action-footer">
                  <button class="btn-pdf" @click="downloadPDF">
                      📄 Baixar Relatório Completo (PDF)
                  </button>
              </div>
          </div>

        </div>
      </main>

      <AppFooter />
    </div>

  </div>
</template>

<style scoped>
/* --- FIX DE LAYOUT --- */
:global(html), :global(body), :global(#app) {
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden; /* Remove rolagem da janela inteira */
}

/* Layout Base */
:global(body) { margin: 0; background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif; }

.app-layout { 
  display: flex; 
  height: 100%;
  width: 100%;
}

/* Sidebar */
.sidebar { 
  width: 260px; 
  flex-shrink: 0; 
  background: white; 
  border-right: 1px solid #e5e7eb; 
  padding: 1.5rem 1rem; 
  display: flex;
  flex-direction: column;
}
.sidebar-logo { width: 120px; margin: 0 auto 2rem; display: block; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; flex: 1; overflow-y: auto; }
.sidebar-nav a, .user-display { display: flex; align-items: center; padding: 0.75rem 1rem; color: #4b5563; text-decoration: none; border-radius: 6px; font-weight: 500; transition: all 0.2s; margin-bottom: 5px; }
.sidebar-nav a:hover { background: #f3f4f6; color: #111; }
.sidebar-nav .active a { background: #eff6ff; color: #2563eb; font-weight: 600; }
.sidebar-nav .icon { margin-right: 10px; min-width: 20px; text-align: center; }
.logout-item { margin-top: auto; border-top: 1px solid #f3f4f6; padding-top: 1rem; } 
.logout-item a { color: #ef4444; }

/* MAIN WRAPPER (Novo container flex column) */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh; /* Altura total da viewport */
  overflow-y: auto; /* Scroll acontece aqui */
}

/* MAIN CONTENT */
.main-content { 
  flex: 1; /* Empurra o footer para baixo */
  padding: 2rem; 
  display: flex; 
  justify-content: center; 
  align-items: flex-start;
  background-color: #f0f2f5;
}

.report-wrapper { max-width: 950px; width: 100%; background: white; padding: 2.5rem; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 2rem; }

/* Header */
.page-header { margin-bottom: 2rem; border-bottom: 2px solid #f3f4f6; padding-bottom: 1rem; }
.content-title { font-size: 1.8rem; margin: 0; color: #111; }
.subtitle { color: #6b7280; margin-top: 5px; }

/* Filtros */
.filters-card { display: flex; gap: 1.5rem; background: #f8fafc; padding: 1.5rem; border-radius: 8px; border: 1px solid #e2e8f0; flex-wrap: wrap; align-items: flex-end; }
.filter-group { display: flex; flex-direction: column; gap: 5px; }
.filter-group.flex-grow { flex: 1; min-width: 200px; }
.filter-group label { font-size: 0.85rem; font-weight: 700; color: #475569; text-transform: uppercase; }
.filter-group select, .fake-input { padding: 0.6rem; border: 1px solid #cbd5e1; border-radius: 6px; background: white; font-size: 1rem; color: #334155; height: 42px; min-width: 200px; }
.fake-input { background: #e2e8f0; display: flex; align-items: center; color: #64748b; cursor: not-allowed; }

/* Stats */
.stats-row { display: flex; gap: 1.5rem; margin-top: 2rem; margin-bottom: 2rem; }
.stat-card { flex: 1; background: linear-gradient(135deg, #eff6ff, #dbeafe); padding: 1.5rem; border-radius: 10px; text-align: center; border: 1px solid #bfdbfe; }
.stat-label { display: block; font-size: 0.9rem; color: #1e40af; font-weight: 600; text-transform: uppercase; margin-bottom: 0.5rem; }
.stat-value { font-size: 2.5rem; font-weight: 800; color: #1e3a8a; }

/* Tabela */
.section-title { font-size: 1.3rem; color: #334155; margin-bottom: 1rem; border-left: 4px solid #3b82f6; padding-left: 10px; }
.table-container { border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; margin-bottom: 2rem; }
.report-table { width: 100%; border-collapse: collapse; }
.report-table th { background: #f8fafc; padding: 1rem; text-align: left; font-size: 0.85rem; color: #64748b; text-transform: uppercase; border-bottom: 1px solid #e2e8f0; }
.report-table td { padding: 1rem; border-bottom: 1px solid #f1f5f9; color: #334155; }
.center { text-align: center; }
.fw-bold { font-weight: 600; }

/* Barras e Badges */
.score-pill { background: #3b82f6; color: white; padding: 2px 8px; border-radius: 10px; font-size: 0.85rem; display: inline-block; min-width: 40px; }
.risk-badge { display: inline-flex; align-items: center; padding: 4px 12px; border-radius: 20px; font-size: 0.85rem; font-weight: 700; }
.dot { width: 8px; height: 8px; border-radius: 50%; margin-right: 6px; background: currentColor; }
.risco-alto { background: #fee2e2; color: #991b1b; }
.risco-medio { background: #ffedd5; color: #9a3412; }
.risco-baixo { background: #dcfce7; color: #166534; }
.risco-desconhecido { background: #f3f4f6; color: #4b5563; }

/* Footer Interno (do card) */
.action-footer { display: flex; justify-content: flex-end; border-top: 1px solid #e5e7eb; padding-top: 1.5rem; }
.btn-pdf { background: #2563eb; color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; }
.btn-pdf:hover { background: #1d4ed8; }

/* Mensagens */
.info-banner { padding: 1.5rem; border-radius: 8px; text-align: center; margin-top: 2rem; font-weight: 500; }
.info-banner.info { background: #eff6ff; color: #1e40af; border: 1px solid #dbeafe; }
.info-banner.error { background: #fee2e2; color: #b91c1c; border: 1px solid #fecaca; }
.loading-state { text-align: center; padding: 3rem; color: #64748b; }
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; position: relative; }
  .main-content { padding: 1rem; overflow: visible; height: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
}
</style>