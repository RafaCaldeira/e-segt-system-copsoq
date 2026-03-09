<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { PlanoDeAcao, Acao, StatusAcao } from '../types/plano.types';
import type { Empresa } from '../types/empresa.types';
import { useRouter, useRoute } from 'vue-router';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
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

// --- ESTADO PARA O MODAL DE CONCLUSÃO ---
const showModalConclusao = ref(false);
const statusParaSalvar = ref<StatusAcao | null>(null);
const acaoEmEdicao = ref<Acao | null>(null);
const formConclusao = ref({
  data: '',
  justificativa: ''
});

const userStore = useUserStore();
const router = useRouter();
const route = useRoute();

const podeEditar = computed(() => userStore.isAdmin || userStore.userRole === 'Psicologo');

const planoAtual = computed(() => planos.value.length > 0 ? planos.value[0] : null);

const nomeEmpresaSelecionada = computed(() => {
  if (userStore.isCliente) return userStore.nomeEmpresa || ''; 
  const emp = empresas.value.find(e => e.id === selectedEmpresaId.value);
  return emp ? emp.nomeEmpresa : 'Selecione uma Empresa';
});

// --- PROGRESSO ---
const progresso = computed(() => {
  if (!planoAtual.value || !planoAtual.value.acoes || planoAtual.value.acoes.length === 0) return 0;
  // Considera concluído como 100% e Em Andamento como 50% (opcional, aqui mantive contagem simples de concluidos)
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
    if (podeEditar.value) {
      const listaEmpresas = await apiService.getEmpresas();
      if (listaEmpresas) empresas.value = listaEmpresas;

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

// --- AÇÕES DO PLANO ---

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
      descricao: novaAcaoTexto.value,
      status: 'Pendente' // Padrão inicial
    });

    if (novaAcao) {
      // Garante campos visuais
      novaAcao.status = 'Pendente'; 
      planoAtual.value.acoes.push(novaAcao);
      novaAcaoTexto.value = '';
    }
  } catch (e) {
    errorMessage.value = "Erro ao adicionar ação.";
  } finally {
    isAddingAcao.value = false;
  }
}

function abrirModalConfirmacao(acao: Acao, statusDestino: StatusAcao) {
  acaoEmEdicao.value = acao;
  statusParaSalvar.value = statusDestino; // Guarda o status que será salvo
  
  // Se for Concluído, sugere a data de hoje. Se for Em Andamento, data fica vazia.
  if (statusDestino === 'Concluido') {
      formConclusao.value.justificativa = acao.justificativa || '';
  } else {
      formConclusao.value.data = '';
  }

  // Carrega justificativa anterior se houver
  formConclusao.value.justificativa = acao.justificativa || ''; 
  
  showModalConclusao.value = true;
}

// Chamado quando o usuário muda o valor no <select>
function onStatusChange(event: Event, acao: Acao) {
  const select = event.target as HTMLSelectElement;
  const novoStatus = select.value as StatusAcao; 

  // Se for Concluído OU Em Andamento, abre a caixinha
  if (novoStatus === 'Concluido' || novoStatus === 'EmAndamento') {
    // Volta o select visualmente para o anterior até confirmar
    select.value = acao.status; 
    
    // Abre o modal passando o NOVO status que o usuário escolheu
    abrirModalConfirmacao(acao, novoStatus);
  } else {
    // Se for Pendente, salva direto (limpa tudo)
    atualizarStatusDireto(acao, novoStatus);
  }
}

async function atualizarStatusDireto(acao: Acao, novoStatus: string) {
  const statusAntigo = acao.status;
  
  // A conversão acontece AQUI DENTRO
  acao.status = novoStatus as StatusAcao; 

  try {
    const sucesso = await apiService.updateStatusAcao(acao.id, {
        status: novoStatus,
        data_conclusao: null,
        justificativa: null
    });
    
    if (!sucesso) throw new Error();
  } catch (e) {
    acao.status = statusAntigo;
    alert("Erro ao atualizar status.");
  }
}


function abrirModalConclusao(acao: Acao) {
  acaoEmEdicao.value = acao;
  formConclusao.value.justificativa = acao.justificativa || '';
  // CORREÇÃO AQUI: Adicione "|| ''" no final
  formConclusao.value.justificativa = acao.justificativa || ''; 
  
  showModalConclusao.value = true;
}

function fecharModal() {
  showModalConclusao.value = false;
  acaoEmEdicao.value = null;
}

async function confirmarConclusao() {
  if (!acaoEmEdicao.value || !statusParaSalvar.value) return;
  
  // Validação simples
  if (!formConclusao.value.justificativa) {
    alert("Por favor, preencha a justificativa.");
    return;
  }

  const acao = acaoEmEdicao.value;
  const novoStatus = statusParaSalvar.value;
  
  // Guarda valores antigos caso precise reverter (backup)
  const statusAntigo = acao.status;
  const justificativaAntiga = acao.justificativa;
  const dataAntiga = acao.data_conclusao;

  // Prepara os dados corretos
  const dataEnviar = novoStatus === 'Concluido' ? formConclusao.value.data : null;

  // --- ATUALIZAÇÃO IMPORTANTE (Correção do PDF) ---
  // Atualizamos a tela e a memória IMEDIATAMENTE para o PDF pegar os dados novos
  acao.status = novoStatus;
  acao.justificativa = formConclusao.value.justificativa; // Agora o PDF vai ler isso!
  acao.data_conclusao = dataEnviar;                       // E isso!

  try {
    const sucesso = await apiService.updateStatusAcao(acao.id, {
        status: novoStatus,
        data_conclusao: dataEnviar, 
        justificativa: formConclusao.value.justificativa
    });

    if (!sucesso) throw new Error();
    
    fecharModal();

  } catch (e) {
    // Se der erro (aquele popup do seu print), revertemos tudo
    acao.status = statusAntigo;
    acao.justificativa = justificativaAntiga;
    acao.data_conclusao = dataAntiga;
    
    console.error(e); // Ajuda a ver o erro real no F12
    alert("Erro ao atualizar status. Verifique se a data é válida.");
  }
}

// PDF Generator
function baixarPDF() {
  if (!planoAtual.value) return;

  const doc = new jsPDF();
  const nomeEmpresaSafe = nomeEmpresaSelecionada.value || 'Empresa';

  // Cabeçalho do PDF
  doc.setFontSize(18);
  doc.text("Plano de Ação de Melhoria", 14, 20);
  
  doc.setFontSize(12);
  doc.setTextColor(100);
  doc.text(`Empresa: ${nomeEmpresaSafe}`, 14, 28);
  
  const dataHoje = new Date().toLocaleDateString('pt-BR');
  doc.setFontSize(10);
  doc.text(`Relatório gerado em: ${dataHoje}`, 14, 34);

  // Prepara os dados com a nova coluna de Histórico
  const dadosTabela = planoAtual.value.acoes.map(acao => {
    // 1. Define o rótulo do Status
    let statusLabel = 'Pendente';
    if (acao.status === 'Concluido') statusLabel = 'Concluído';
    if (acao.status === 'EmAndamento') statusLabel = 'Em Andamento';
    
    // 2. Monta o texto do Histórico (Justificativa + Data)
    let historico = acao.justificativa || ''; // Começa com a justificativa ou vazio

    // Se estiver concluído e tiver data, coloca a data antes
    if (acao.status === 'Concluido' && acao.data_conclusao) {
        const dataFmt = new Date(acao.data_conclusao).toLocaleDateString('pt-BR');
        // Adiciona a data no topo da célula
        historico = `[Concluído em: ${dataFmt}]\n${historico}`;
    } else if (!historico && acao.status !== 'Pendente') {
        historico = '-'; // Se não tiver texto mas não for pendente
    }

    return [
      acao.descricao,
      statusLabel,
      historico // Nova coluna
    ];
  });

  // Gera a tabela
  autoTable(doc, {
    startY: 40,
    // Adicionamos a nova coluna no cabeçalho
    head: [['Descrição da Ação', 'Situação', 'Histórico / Evidências']], 
    body: dadosTabela,
    theme: 'grid',
    headStyles: { fillColor: [37, 99, 235] },
    styles: { fontSize: 9, cellPadding: 3, valign: 'middle' },
    
    // Ajuste de largura das colunas para ficar bonito
    columnStyles: {
        0: { cellWidth: 60 }, // Descrição
        1: { cellWidth: 35, halign: 'center' }, // Situação
        2: { cellWidth: 'auto' } // Histórico pega o resto do espaço
    },

    // Cores dos status (mantivemos sua lógica legal de cores)
    didParseCell: function (data) {
      if (data.section === 'body' && data.column.index === 1) {
        const texto = data.cell.raw;
        if (texto === 'Concluído') {
          data.cell.styles.textColor = [34, 197, 94]; // Verde
          data.cell.styles.fontStyle = 'bold';
        } else if (texto === 'Em Andamento') {
          data.cell.styles.textColor = [234, 88, 12]; // Laranja
        } else {
          data.cell.styles.textColor = [100, 116, 139]; // Cinza
        }
      }
    }
  });

  const nomeArquivo = `Plano_${nomeEmpresaSafe.replace(/\s+/g, '_')}_${dataHoje.replace(/\//g, '-')}.pdf`;
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
                
                <div v-for="acao in planoAtual.acoes" :key="acao.id" 
                     class="action-card" 
                     :class="{'card-concluido': acao.status === 'Concluido', 'card-andamento': acao.status === 'EmAndamento'}">
                  
                  <div class="action-content">
                    <div class="status-icon" :class="acao.status">
                      <span v-if="acao.status === 'Concluido'">✔</span>
                      <span v-else-if="acao.status === 'EmAndamento'">⏳</span>
                      <span v-else>⭕</span>
                    </div>
                    
                    <div class="action-text-group">
                       <span class="action-desc" :class="{ struck: acao.status === 'Concluido' }">{{ acao.descricao }}</span>
                       <small v-if="acao.status === 'Concluido' && acao.data_conclusao" class="data-badge">
                         Concluído em: {{ new Date(acao.data_conclusao).toLocaleDateString('pt-BR') }}
                       </small>
                    </div>
                  </div>
                  
                  <div class="status-selector-wrapper">
                    <select 
                      :value="acao.status" 
                      @change="onStatusChange($event, acao)"
                      :disabled="!podeEditar"
                      class="status-select"
                      :class="acao.status"
                    >
                      <option value="Pendente">Pendente</option>
                      <option value="EmAndamento">Em Andamento (Parcial)</option>
                      <option value="Concluido">Concluído</option>
                    </select>
                  </div>

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

    <div v-if="showModalConclusao" class="modal-overlay">
      <div class="modal-card">
        <div class="modal-header">
          <h3>
              {{ statusParaSalvar === 'Concluido' ? 'Concluir Ação' : 'Justificar Andamento' }}
          </h3>
          <button class="btn-close" @click="fecharModal">✕</button>
        </div>
        
        <div class="modal-body">
          <p class="modal-desc-action">"{{ acaoEmEdicao?.descricao }}"</p>
          
          <div v-if="statusParaSalvar === 'Concluido'">
              <label>Data da Conclusão:</label>
              <input type="date" v-model="formConclusao.data" class="modal-input" />
          </div>
          
          <label>
              {{ statusParaSalvar === 'Concluido' ? 'Evidência / Justificativa (Obrigatório):' : 'Por que está pendente/em andamento?' }}
          </label>
          <textarea 
            v-model="formConclusao.justificativa" 
            rows="4" 
            class="modal-textarea"
            placeholder="Descreva os detalhes..."
          ></textarea>
        </div>

        <div class="modal-footer">
          <button class="btn-cancel" @click="fecharModal">Cancelar</button>
          <button class="btn-confirm" @click="confirmarConclusao">
            {{ statusParaSalvar === 'Concluido' ? 'Confirmar Conclusão' : 'Salvar Alteração' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>
<style scoped>
/* Manter os estilos globais e layout que você já tinha... */
:global(html), :global(body), :global(#app) { height: 100%; margin: 0; padding: 0; overflow: hidden; }
:global(body) { background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif; }
.app-layout { display: flex; height: 100%; width: 100%; }
.main-wrapper { flex: 1; display: flex; flex-direction: column; height: 100vh; overflow-y: auto; }
.main-content { flex: 1; padding: 2rem; display: flex; justify-content: center; align-items: flex-start; background-color: #f0f2f5; }
.content-wrapper { max-width: 900px; width: 100%; background: white; padding: 2.5rem; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 3rem; }

/* ... Styles do Header e botões mantidos ... */
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; border-bottom: 2px solid #f3f4f6; padding-bottom: 1.5rem; flex-wrap: wrap; gap: 1rem; }
.content-title { font-size: 1.8rem; margin: 0; color: #111; }
.company-subtitle { font-size: 1.1rem; margin: 5px 0 0 0; color: #6b7280; font-weight: 500; }
.header-actions { display: flex; gap: 15px; align-items: center; flex-wrap: wrap; }
.company-selector select { padding: 0.6rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 1rem; min-width: 250px; cursor: pointer; height: 42px; }
.btn-pdf { background-color: #ef4444; color: white; border: none; padding: 0.6rem 1.2rem; border-radius: 6px; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: background-color 0.2s; height: 42px; }
.btn-pdf:hover { background-color: #dc2626; }

/* Progress Card Styles */
.progress-card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 1.5rem; margin-bottom: 2rem; }
.progress-info { display: flex; justify-content: space-between; margin-bottom: 0.5rem; font-weight: 700; color: #334155; }
.progress-track { background: #e2e8f0; height: 12px; border-radius: 6px; overflow: hidden; margin-bottom: 0.5rem; }
.progress-fill { height: 100%; background: linear-gradient(90deg, #22c55e, #16a34a); transition: width 0.4s ease; }
.progress-details { text-align: right; font-size: 0.85rem; color: #64748b; }

/* --- NOVOS ESTILOS PARA CARDS E SELECT --- */
.action-card { display: flex; justify-content: space-between; align-items: flex-start; padding: 1rem; border: 1px solid #e5e7eb; border-radius: 8px; margin-bottom: 0.75rem; transition: all 0.2s; background: white; flex-wrap: wrap; gap: 10px; }
.action-card:hover { border-color: #cbd5e1; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }

.card-concluido { background-color: #f0fdf4; border-color: #bbf7d0; }
.card-andamento { background-color: #fff7ed; border-color: #fed7aa; } /* Laranja claro */

.action-content { display: flex; align-items: flex-start; gap: 12px; flex: 1; min-width: 250px; }
.action-text-group { display: flex; flex-direction: column; }
.data-badge { font-size: 0.8rem; color: #16a34a; font-weight: 600; margin-top: 2px; }

.status-icon { font-size: 1.2rem; width: 30px; display: flex; justify-content: center; }
.status-icon.Concluido { color: #22c55e; }
.status-icon.EmAndamento { color: #f97316; }

.action-desc { font-size: 1rem; color: #334155; line-height: 1.4; }
.action-desc.struck { text-decoration: line-through; color: #94a3b8; }

/* Estilização do Select de Status */
.status-selector-wrapper { min-width: 140px; }
.status-select { 
  width: 100%; 
  padding: 8px 12px; 
  border-radius: 6px; 
  border: 1px solid #cbd5e1; 
  font-size: 0.9rem; 
  cursor: pointer; 
  font-weight: 600; 
  outline: none;
}
/* Cores do Select baseadas no valor */
.status-select.Pendente { background-color: #f1f5f9; color: #475569; }
.status-select.EmAndamento { background-color: #fff7ed; color: #c2410c; border-color: #fdba74; }
.status-select.Concluido { background-color: #dcfce7; color: #15803d; border-color: #86efac; }

/* Input área e Loading/Error mantidos */
.add-action-area { margin-top: 2rem; border-top: 2px dashed #e2e8f0; padding-top: 1.5rem; }
.add-action-area h4 { margin: 0 0 0.5rem 0; color: #475569; }
.input-group { display: flex; gap: 10px; }
.input-group input { flex: 1; padding: 0.75rem; border: 1px solid #cbd5e1; border-radius: 6px; }
.input-group button { padding: 0.75rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; }
.input-group button:disabled { background: #94a3b8; cursor: not-allowed; }
.empty-state { text-align: center; padding: 3rem; color: #64748b; font-size: 1.1rem; }
.loading-state { text-align: center; padding: 3rem; color: #64748b; }
.spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #e5e7eb; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-right: 10px; vertical-align: middle; }
@keyframes spin { to { transform: rotate(360deg); } }
.error-banner { background-color: #fee2e2; border: 1px solid #fecaca; color: #b91c1c; padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; text-align: center; font-weight: 500; }
.btn-create { margin-top: 1rem; padding: 0.8rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; }
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

/* --- CSS DO MODAL --- */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.5); z-index: 1000;
  display: flex; justify-content: center; align-items: center;
  padding: 20px;
}
.modal-card {
  background: white; width: 100%; max-width: 500px;
  border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.2);
  display: flex; flex-direction: column; overflow: hidden;
  animation: modalUp 0.3s ease-out;
}
@keyframes modalUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }

.modal-header {
  padding: 1.5rem; border-bottom: 1px solid #e2e8f0;
  display: flex; justify-content: space-between; align-items: center;
  background-color: #f8fafc;
}
.modal-header h3 { margin: 0; color: #1e293b; }
.btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #64748b; }

.modal-body { padding: 1.5rem; display: flex; flex-direction: column; gap: 10px; }
.modal-desc-action { font-style: italic; color: #64748b; background: #f1f5f9; padding: 10px; border-radius: 6px; margin-bottom: 15px; border-left: 4px solid #3b82f6; }
.modal-body label { font-weight: 600; color: #334155; font-size: 0.9rem; margin-top: 5px; }
.modal-input, .modal-textarea { padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; font-family: inherit; }
.modal-textarea { resize: vertical; }

.modal-footer {
  padding: 1rem 1.5rem; background-color: #f8fafc; border-top: 1px solid #e2e8f0;
  display: flex; justify-content: flex-end; gap: 10px;
}
.btn-cancel { padding: 10px 20px; border: 1px solid #cbd5e1; background: white; border-radius: 6px; cursor: pointer; font-weight: 600; color: #475569; }
.btn-confirm { padding: 10px 20px; background: #22c55e; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600; }
.btn-confirm:hover { background: #16a34a; }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
  .action-card { flex-direction: column; align-items: stretch; }
  .status-selector-wrapper { width: 100%; margin-top: 10px; }
}
</style>