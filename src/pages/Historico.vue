<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { useRouter, useRoute } from 'vue-router'; // <--- 1. ADICIONEI useRoute AQUI
import type { DisparoHistoricoDto } from '../types/disparo.types';
import type { Empresa } from '../types/empresa.types';
import { apiService } from '../services/api.service';
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

const userStore = useUserStore();
const router = useRouter();
const route = useRoute(); // <--- 2. INICIALIZEI A ROTA AQUI

// Estado
const historicoCompleto = ref<DisparoHistoricoDto[]>([]);
const empresas = ref<Empresa[]>([]);
const selectedEmpresaId = ref<number | null>(null);
const isLoading = ref(true);

// Permissões
const podeFiltrar = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');
const podeVerAcoesTabela = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');

onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }
  
  isLoading.value = true;
  try {
    // 1. Buscar Empresas (se for admin/psicólogo)
    if (podeFiltrar.value) {
      const listaEmpresas = await apiService.getEmpresas();
      if (listaEmpresas) empresas.value = listaEmpresas;
    } else {
      selectedEmpresaId.value = userStore.empresaId;
    }

    // 2. Buscar Histórico
    const data = await apiService.getHistoricoDisparos();
    if (data) {
      historicoCompleto.value = data;
    }

    // --- 3. A CORREÇÃO: VERIFICAR URL ---
    // Se a URL tiver ?empresaId=5, vamos selecionar automaticamente
    if (podeFiltrar.value && route.query.empresaId) {
      const idDaUrl = Number(route.query.empresaId);
      
      // Verifica se o ID é válido e existe na lista carregada
      if (!isNaN(idDaUrl)) {
        selectedEmpresaId.value = idDaUrl;
      }
    }
    // ------------------------------------

  } finally {
    isLoading.value = false;
  }
});

// --- COMPUTEDS (Mantive igual) ---
const historicoFiltrado = computed(() => {
  if (!selectedEmpresaId.value) return [];
  return historicoCompleto.value.filter(h => h.empresaId === selectedEmpresaId.value);
});

const estatisticasGerais = computed(() => {
  const total = historicoFiltrado.value.length;
  const respondidos = historicoFiltrado.value.filter(h => h.respondido).length;
  const porcentagem = total > 0 ? Math.round((respondidos / total) * 100) : 0;
  return { total, respondidos, porcentagem };
});

interface EstatisticaSetor {
  nome: string; total: number; respondidos: number; porcentagem: number;
}

const progressoPorSetor = computed<EstatisticaSetor[]>(() => {
  const grupos: Record<string, { total: number; respondidos: number }> = {};
  historicoFiltrado.value.forEach(item => {
    const nomeSetor = item.setor || 'Setor Não Definido'; 
    if (!grupos[nomeSetor]) grupos[nomeSetor] = { total: 0, respondidos: 0 };
    grupos[nomeSetor].total++;
    if (item.respondido) grupos[nomeSetor].respondidos++;
  });
  return Object.keys(grupos).map(setor => {
    const dados = grupos[setor]!; 
    return {
      nome: setor,
      total: dados.total,
      respondidos: dados.respondidos,
      porcentagem: dados.total > 0 ? Math.round((dados.respondidos / dados.total) * 100) : 0
    };
  });
});

function copiarLink(token: string) {
  const urlCompleta = `${window.location.origin}/responder/${token}`;
  navigator.clipboard.writeText(urlCompleta).then(() => alert("Link copiado!"));
}

function formatarData(dataIso: string) {
  if (!dataIso) return '-';
  return new Date(dataIso).toLocaleDateString('pt-BR');
}
</script>

<template>
  <div class="app-layout">
    
    <AppSidebar />

    <div class="main-wrapper">
      <main class="main-content">
        <div class="content-wrapper">
          <div class="header-area">
              <div>
                  <h1 class="content-title">Histórico e Progresso</h1>
                  <p class="desc">Monitore as respostas por empresa e setor.</p>
              </div>

              <div v-if="podeFiltrar" class="company-select">
                  <select v-model="selectedEmpresaId">
                      <option :value="null" disabled>Selecione uma Empresa</option>
                      <option v-for="emp in empresas" :key="emp.id" :value="emp.id">
                          {{ emp.nomeEmpresa }}
                      </option>
                  </select>
              </div>
          </div>

          <div v-if="isLoading" class="loading">Carregando dados...</div>

          <div v-else>
              <div v-if="!selectedEmpresaId" class="empty-state">
                  Selecione uma empresa acima para visualizar o progresso.
              </div>

              <div v-else>
                  <div class="summary-card fade-in">
                      <div class="summary-text">
                          <h3>Progresso Geral</h3>
                          <span class="big-number">
                              {{ estatisticasGerais.respondidos }} <span class="divider">/</span> {{ estatisticasGerais.total }}
                          </span>
                          <p>Funcionários responderam</p>
                      </div>
                      <div class="circular-chart">
                          <div class="circle" :style="{ background: `conic-gradient(#3b82f6 ${estatisticasGerais.porcentagem}%, #e0e0e0 0)` }">
                              <div class="inner-circle">{{ estatisticasGerais.porcentagem }}%</div>
                          </div>
                      </div>
                  </div>

                  <h3 class="section-subtitle">Progresso por Setor</h3>
                  <div class="sectors-grid fade-in">
                      <div v-for="setor in progressoPorSetor" :key="setor.nome" class="sector-card">
                          <div class="sector-header">
                              <span class="sector-name">{{ setor.nome }}</span>
                              <span class="sector-stats">{{ setor.respondidos }} / {{ setor.total }}</span>
                          </div>
                          <div class="progress-bar-bg">
                              <div class="progress-bar-fill" :style="{ width: setor.porcentagem + '%' }"></div>
                          </div>
                      </div>
                  </div>

                  <h3 class="section-subtitle">Detalhamento dos Envios</h3>
                  <table class="history-table fade-in">
                      <thead>
                      <tr>
                          <th>Funcionário / Setor</th>
                          <th>Questionário</th>
                          <th>Data Envio</th>
                          <th>Status</th>
                          <th v-if="podeVerAcoesTabela">Ações</th>
                      </tr>
                      </thead>
                      <tbody>
                      <tr v-for="item in historicoFiltrado" :key="item.id">
                          <td>
                              <strong>{{ item.nomeFuncionario }}</strong><br>
                              <span class="small-info">{{ item.setor || 'Sem setor' }} | {{ item.emailFuncionario }}</span>
                          </td>
                          <td>{{ item.tituloQuestionario }}</td>
                          <td>{{ formatarData(item.dataEnvio) }}</td>
                          <td>
                              <span class="status-badge" :class="item.respondido ? 'respondido' : 'pendente'">
                                  {{ item.respondido ? 'Respondido' : 'Pendente' }}
                              </span>
                          </td>
                          <td v-if="podeVerAcoesTabela">
                              <button v-if="!item.respondido" class="btn-copy" @click="copiarLink(item.link)">🔗 Copiar</button>
                          </td>
                      </tr>
                      </tbody>
                  </table>
                  <div v-if="historicoFiltrado.length === 0" class="no-data">
                      Nenhum envio encontrado para esta empresa.
                  </div>
              </div>
          </div>

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

/* --- MAIN WRAPPER (Novo container flex column) --- */
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

.content-wrapper { max-width: 1000px; width: 100%; margin: 0 auto; padding-bottom: 3rem; }

.header-area { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; flex-wrap: wrap; }
.content-title { font-size: 1.8rem; margin: 0; color: #111; }
.desc { color: #666; margin: 5px 0 0 0; }

.company-select select { padding: 0.8rem; border: 1px solid #d1d5db; border-radius: 8px; font-size: 1rem; min-width: 250px; cursor: pointer; background: white; }

.summary-card { background: white; padding: 2rem; border-radius: 12px; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 2rem; }
.summary-text h3 { margin: 0 0 0.5rem 0; color: #6b7280; font-size: 0.9rem; text-transform: uppercase; letter-spacing: 0.05em; }
.big-number { font-size: 3rem; font-weight: 800; color: #111; }
.big-number .divider { color: #d1d5db; font-weight: 400; font-size: 2rem; }
.circular-chart .circle { width: 80px; height: 80px; border-radius: 50%; display: flex; align-items: center; justify-content: center; position: relative; }
.circular-chart .inner-circle { width: 60px; height: 60px; background: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 1.1rem; color: #3b82f6; }

.section-subtitle { font-size: 1.2rem; color: #374151; margin-bottom: 1rem; margin-top: 2rem; }
.sectors-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; margin-bottom: 2rem; }
.sector-card { background: white; padding: 1.5rem; border-radius: 8px; border: 1px solid #e5e7eb; box-shadow: 0 1px 2px rgba(0,0,0,0.05); }
.sector-header { display: flex; justify-content: space-between; margin-bottom: 0.8rem; font-weight: 600; color: #374151; }
.progress-bar-bg { background: #f3f4f6; height: 10px; border-radius: 5px; overflow: hidden; }
.progress-bar-fill { background: #3b82f6; height: 100%; transition: width 0.5s ease; }

.history-table { width: 100%; border-collapse: separate; border-spacing: 0; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); margin-bottom: 2rem; }
.history-table th { background: #f9fafb; padding: 1rem; text-align: left; font-size: 0.85rem; color: #6b7280; text-transform: uppercase; font-weight: 600; border-bottom: 1px solid #e5e7eb; }
.history-table td { padding: 1rem; border-bottom: 1px solid #f3f4f6; color: #4b5563; }
.small-info { font-size: 0.85rem; color: #9ca3af; }
.status-badge { padding: 0.25rem 0.75rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
.status-badge.respondido { background: #dcfce7; color: #166534; }
.status-badge.pendente { background: #fef3c7; color: #92400e; }
.btn-copy { background: none; border: 1px solid #d1d5db; padding: 0.4rem 0.8rem; border-radius: 6px; cursor: pointer; color: #374151; font-size: 0.85rem; }
.btn-copy:hover { background: #f3f4f6; border-color: #9ca3af; }
.no-data { text-align: center; padding: 2rem; color: #888; }

.empty-state { text-align: center; padding: 4rem; background: white; border-radius: 12px; color: #6b7280; border: 2px dashed #e5e7eb; }
.loading { text-align: center; padding: 3rem; color: #6b7280; }
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
}
</style>