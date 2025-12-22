<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { PlanoDeAcao, Acao } from '../types/plano.types';
import type { Empresa } from '../types/empresa.types';
import { useRouter, useRoute } from 'vue-router';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

// --- Estado ---
const planos = ref<PlanoDeAcao[]>([]);
const empresas = ref<Empresa[]>([]); 
const selectedEmpresaId = ref<number | null>(null);

const isLoading = ref(false);
const errorMessage = ref<string | null>(null);

// Estado para Nova Ação
const novaAcaoTexto = ref('');
const isAddingAcao = ref(false);

const userStore = useUserStore();
const router = useRouter();
const route = useRoute();

// --- PERMISSÕES ---
// Define quem pode Criar Plano, Adicionar Ação e Concluir Ação
const podeEditar = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');

// --- Lógica do Plano Atual ---
const planoAtual = computed(() => planos.value.length > 0 ? planos.value[0] : null);

const nomeEmpresaSelecionada = computed(() => {
  if (userStore.isCliente) return userStore.nomeEmpresa || ''; 
  const emp = empresas.value.find(e => e.id === selectedEmpresaId.value);
  return emp ? emp.nomeEmpresa : 'Selecione uma Empresa';
});

// --- PROGRESSO ---
const progresso = computed(() => {
  if (!planoAtual.value || !planoAtual.value.acoes || planoAtual.value.acoes.length === 0) return 0;
  const concluidas = planoAtual.value.acoes.filter(a => a.status === 'Concluido').length;
  return Math.round((concluidas / planoAtual.value.acoes.length) * 100);
});

const acoesConcluidasCount = computed(() => 
  planoAtual.value ? planoAtual.value.acoes.filter(a => a.status === 'Concluido').length : 0
);

const totalAcoesCount = computed(() => 
  planoAtual.value ? planoAtual.value.acoes.length : 0
);

// --- Inicialização ---
onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }

  try {
    // Se for Admin ou Psicólogo, carrega a lista de empresas para o select
    if (podeEditar.value) {
      const listaEmpresas = await apiService.getEmpresas();
      if (listaEmpresas) empresas.value = listaEmpresas;

      // Se veio com ID na URL (clicou em Gerir no Dashboard), seleciona automático
      const queryId = route.query.empresaId;
      if (queryId) {
        selectedEmpresaId.value = Number(queryId);
      }
    } else {
      selectedEmpresaId.value = userStore.empresaId;
    }
  } catch (e) {
    errorMessage.value = "Erro ao carregar dados iniciais.";
  }
});

watch(selectedEmpresaId, async (newId) => {
  if (newId) {
    await carregarPlanoDaEmpresa(newId);
  } else {
    planos.value = [];
  }
});

async function carregarPlanoDaEmpresa(empresaId: number) {
  isLoading.value = true;
  errorMessage.value = null;
  planos.value = []; 
  try {
    const data = await apiService.getPlanosPorEmpresa(empresaId);
    if (data) planos.value = data;
  } catch (e) {
    errorMessage.value = "Não foi possível carregar o plano desta empresa.";
  } finally {
    isLoading.value = false;
  }
}

// --- AÇÕES ---

async function criarPlanoInicial() {
  if (!podeEditar.value || !selectedEmpresaId.value) return;
  
  try {
    const sucesso = await apiService.createPlano({
      titulo: 'Plano de Ação de Melhoria - COPSOQ',
      descricao: 'Ações baseadas nos resultados da avaliação de riscos psicossociais.',
      empresaID: selectedEmpresaId.value
    });
    
    if (sucesso) await carregarPlanoDaEmpresa(selectedEmpresaId.value);
  } catch (e) {
    errorMessage.value = "Erro ao criar plano.";
  }
}

async function adicionarAcao() {
  if (!novaAcaoTexto.value || !planoAtual.value) return;
  isAddingAcao.value = true;
  errorMessage.value = null;
  
  try {
    const novaAcao = await apiService.addAcao(planoAtual.value.id, {
      descricao: novaAcaoTexto.value
    });

    if (novaAcao) {
      if (!novaAcao.status) novaAcao.status = 'Pendente';
      planoAtual.value.acoes.push(novaAcao);
      novaAcaoTexto.value = '';
    }
  } catch (e) {
    errorMessage.value = "Erro ao adicionar ação.";
  } finally {
    isAddingAcao.value = false;
  }
}

async function toggleStatus(acao: Acao) {
  if (!podeEditar.value) return; 
  
  const novoStatus = acao.status === 'Concluido' ? 'Pendente' : 'Concluido';
  const statusAntigo = acao.status;

  // Atualização Otimista
  acao.status = novoStatus; 

  try {
    const sucesso = await apiService.updateStatusAcao(acao.id, novoStatus);
    if (!sucesso) {
      acao.status = statusAntigo;
      alert("Erro ao salvar status.");
    }
  } catch (e) {
    acao.status = statusAntigo;
    console.error(e);
  }
}

// PDF Generator
function baixarPDF() {
  if (!planoAtual.value) return;

  const doc = new jsPDF();
  const nomeEmpresaSafe = nomeEmpresaSelecionada.value || 'Empresa';

  doc.setFontSize(18);
  doc.text("Plano de Ação de Melhoria", 14, 20);
  
  doc.setFontSize(12);
  doc.setTextColor(100);
  doc.text(`Empresa: ${nomeEmpresaSafe}`, 14, 28);
  
  const dataHoje = new Date().toLocaleDateString('pt-BR');
  doc.setFontSize(10);
  doc.text(`Gerado em: ${dataHoje}`, 14, 34);

  const dadosTabela = planoAtual.value.acoes.map(acao => [
    acao.descricao,
    acao.status === 'Concluido' ? 'Concluído' : 'Pendente'
  ]);

  autoTable(doc, {
    startY: 40,
    head: [['Descrição da Ação / Melhoria', 'Situação']],
    body: dadosTabela,
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 10, cellPadding: 3 },
    didParseCell: function (data) {
      if (data.section === 'body' && data.column.index === 1) {
        const texto = data.cell.raw;
        if (texto === 'Concluído') {
          data.cell.styles.textColor = [34, 197, 94];
          data.cell.styles.fontStyle = 'bold';
        } else {
          data.cell.styles.textColor = [220, 38, 38];
        }
      }
    }
  });

  const nomeArquivo = `Plano_${nomeEmpresaSafe.replace(/\s+/g, '_')}.pdf`;
  doc.save(nomeArquivo);
}
</script>

<template>
  <div class="app-layout">
    
    <AppSidebar />

    <div class="main-wrapper">
      <main class="main-content">
        <div class="content-wrapper">
          
          <header class="page-header">
            <div>
              <h1 class="content-title">Plano de Ação</h1>
              <h2 class="company-subtitle" v-if="selectedEmpresaId">
                  🏢 {{ nomeEmpresaSelecionada }}
              </h2>
            </div>
            
            <div class="header-actions">
              <button v-if="planoAtual" @click="baixarPDF" class="btn-pdf" title="Baixar lista em PDF">
                📄 Baixar PDF
              </button>
              
              <div v-if="podeEditar" class="company-selector">
                <select v-model="selectedEmpresaId">
                  <option :value="null" disabled>-- Selecione uma Empresa --</option>
                  <option v-for="emp in empresas" :key="emp.id" :value="emp.id">
                    {{ emp.nomeEmpresa }}
                  </option>
                </select>
              </div>
            </div>
          </header>

          <div v-if="errorMessage" class="error-banner">⚠️ {{ errorMessage }}</div>
          
          <div v-if="isLoading" class="loading-state">
            <div class="spinner"></div> Carregando plano de ação...
          </div>

          <div v-else>
            
            <div v-if="!selectedEmpresaId" class="empty-state">
              <p>⬅️ Selecione uma empresa acima para visualizar o plano.</p>
            </div>
            
            <div v-else-if="!planoAtual" class="empty-state">
              <p>A empresa <strong>{{ nomeEmpresaSelecionada }}</strong> ainda não possui um Plano de Ação.</p>
              <button v-if="podeEditar" @click="criarPlanoInicial" class="btn-create">
                ✨ Criar Plano Inicial
              </button>
            </div>

            <div v-else class="plan-container fade-in">
              
              <div class="progress-card">
                <div class="progress-info">
                  <span class="progress-title">Progresso Geral</span>
                  <span class="progress-perc">{{ progresso }}%</span>
                </div>
                <div class="progress-track">
                  <div class="progress-fill" :style="{ width: progresso + '%' }"></div>
                </div>
                <div class="progress-details">{{ acoesConcluidasCount }} de {{ totalAcoesCount }} ações concluídas</div>
              </div>

              <div class="actions-list">
                <h3 class="section-title">Ações e Melhorias</h3>
                
                <div v-if="planoAtual.acoes.length === 0" class="no-actions">Nenhuma ação cadastrada. Adicione melhorias abaixo.</div>
                
                <div v-for="acao in planoAtual.acoes" :key="acao.id" class="action-card" :class="{ 'card-concluido': acao.status === 'Concluido' }">
                  <div class="action-content">
                    <div class="check-circle" :class="{ checked: acao.status === 'Concluido' }">{{ acao.status === 'Concluido' ? '✔' : '' }}</div>
                    <span class="action-desc" :class="{ struck: acao.status === 'Concluido' }">{{ acao.descricao }}</span>
                  </div>
                  
                  <button 
                    @click="toggleStatus(acao)"
                    class="btn-status"
                    :class="acao.status === 'Concluido' ? 'btn-concluido' : 'btn-pendente'"
                    :disabled="!podeEditar" 
                    :title="podeEditar ? 'Clique para alterar status' : 'Apenas leitura'"
                  >
                    {{ acao.status === 'Concluido' ? 'Concluído' : 'Pendente' }}
                  </button>
                </div>
              </div>

              <div v-if="podeEditar" class="add-action-area">
                <h4>Adicionar Nova Melhoria</h4>
                <div class="input-group">
                  <input v-model="novaAcaoTexto" type="text" placeholder="Descreva a ação a ser tomada..." @keyup.enter="adicionarAcao" />
                  <button @click="adicionarAcao" :disabled="isAddingAcao || !novaAcaoTexto">{{ isAddingAcao ? '...' : '+ Adicionar' }}</button>
                </div>
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

.app-layout { display: flex; height: 100%; width: 100%; }

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

.content-wrapper { max-width: 900px; width: 100%; background: white; padding: 2.5rem; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 3rem; }

/* HEADER E BOTÕES */
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; border-bottom: 2px solid #f3f4f6; padding-bottom: 1.5rem; flex-wrap: wrap; gap: 1rem; }
.content-title { font-size: 1.8rem; margin: 0; color: #111; }
.company-subtitle { font-size: 1.1rem; margin: 5px 0 0 0; color: #6b7280; font-weight: 500; }

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
  flex-wrap: wrap;
}

.company-selector select { padding: 0.6rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 1rem; min-width: 250px; cursor: pointer; height: 42px; }

.btn-pdf {
  background-color: #ef4444;
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.2s;
  height: 42px;
}
.btn-pdf:hover { background-color: #dc2626; }

/* DEMAIS ESTILOS */
.error-banner { background-color: #fee2e2; border: 1px solid #fecaca; color: #b91c1c; padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; text-align: center; font-weight: 500; }

.progress-card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 1.5rem; margin-bottom: 2rem; }
.progress-info { display: flex; justify-content: space-between; margin-bottom: 0.5rem; font-weight: 700; color: #334155; }
.progress-track { background: #e2e8f0; height: 12px; border-radius: 6px; overflow: hidden; margin-bottom: 0.5rem; }
.progress-fill { height: 100%; background: linear-gradient(90deg, #22c55e, #16a34a); transition: width 0.4s ease; }
.progress-details { text-align: right; font-size: 0.85rem; color: #64748b; }

.section-title { font-size: 1.2rem; color: #334155; margin-bottom: 1rem; }
.action-card { display: flex; justify-content: space-between; align-items: center; padding: 1rem; border: 1px solid #e5e7eb; border-radius: 8px; margin-bottom: 0.75rem; transition: all 0.2s; background: white; }
.action-card:hover { border-color: #cbd5e1; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
.card-concluido { background-color: #f0fdf4; border-color: #bbf7d0; }
.action-content { display: flex; align-items: center; gap: 12px; flex: 1; }
.check-circle { width: 24px; height: 24px; border: 2px solid #cbd5e1; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 14px; color: white; }
.check-circle.checked { background-color: #22c55e; border-color: #22c55e; }
.action-desc { font-size: 1rem; color: #334155; }
.action-desc.struck { text-decoration: line-through; color: #94a3b8; }

.btn-status { padding: 0.5rem 1rem; border-radius: 6px; border: none; font-weight: 600; cursor: pointer; transition: all 0.2s; min-width: 100px; }
.btn-pendente { background-color: #7c3aed; color: white; }
.btn-pendente:hover { background-color: #6d28d9; }
.btn-concluido { background-color: #22c55e; color: white; }
.btn-concluido:hover { background-color: #16a34a; }
.btn-status:disabled { opacity: 0.7; cursor: not-allowed; }

.add-action-area { margin-top: 2rem; border-top: 2px dashed #e2e8f0; padding-top: 1.5rem; }
.add-action-area h4 { margin: 0 0 0.5rem 0; color: #475569; }
.input-group { display: flex; gap: 10px; }
.input-group input { flex: 1; padding: 0.75rem; border: 1px solid #cbd5e1; border-radius: 6px; }
.input-group button { padding: 0.75rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; }
.input-group button:hover { background: #1d4ed8; }
.input-group button:disabled { background: #94a3b8; cursor: not-allowed; }

.empty-state { text-align: center; padding: 3rem; color: #64748b; font-size: 1.1rem; }
.loading-state { text-align: center; padding: 3rem; color: #64748b; }
.spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #e5e7eb; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-right: 10px; vertical-align: middle; }
@keyframes spin { to { transform: rotate(360deg); } }

.btn-create { margin-top: 1rem; padding: 0.8rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; }
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
}
</style>