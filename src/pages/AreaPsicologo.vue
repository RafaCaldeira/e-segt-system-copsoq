<script setup lang="ts">
import { ref, onMounted } from 'vue'; // Removido computed, useUserStore, useRouter daqui pois foi para a sidebar
import { apiService } from '../services/api.service';
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue'; // <--- 1. IMPORTAR

// --- TIPOS ---
interface QuestionarioResumoDto { titulo: string; dataResposta: string; tokenAcesso: string; }
interface FuncionarioListaDto { id: number; nome: string; cargo: string; questionariosRespondidos: QuestionarioResumoDto[]; }
interface EmpresaSimplesDto { id: number; nomeEmpresa: string; setorAtuacao: string; }

// --- CONFIG ---
const API_URL = 'http://localhost:5258'; 

// --- ESTADO ---
const viewState = ref<'empresas' | 'funcionarios'>('empresas');
const isLoading = ref(false);
const empresas = ref<EmpresaSimplesDto[]>([]);
const funcionarios = ref<FuncionarioListaDto[]>([]);
const selectedEmpresa = ref<EmpresaSimplesDto | null>(null);
const showModal = ref(false);
const funcionarioSelecionado = ref<FuncionarioListaDto | null>(null);

// --- CICLO DE VIDA ---
onMounted(async () => {
  await carregarEmpresas();
});

// --- AÇÕES ---
async function carregarEmpresas() {
  isLoading.value = true;
  viewState.value = 'empresas';
  try {
    const resp = await apiService.getEmpresasParaPsicologo();
    empresas.value = resp || [];
  } catch (error) {
    console.error(error);
    alert('Erro ao carregar empresas.');
  } finally {
    isLoading.value = false;
  }
}
//selecionar Empresa e carregar funcionários
async function selecionarEmpresa(empresa: EmpresaSimplesDto) {
  selectedEmpresa.value = empresa;
  isLoading.value = true;
  try {
    const data = await apiService.getListaFuncionarios(empresa.id);
    funcionarios.value = data || [];
    viewState.value = 'funcionarios';
  } catch (error) {
    console.error("Erro ao buscar funcionários:", error);
    alert('Erro ao carregar lista de funcionários.');
  } finally {
    isLoading.value = false;
  }
}

function voltarParaEmpresas() {
  viewState.value = 'empresas';
  selectedEmpresa.value = null;
  funcionarios.value = [];
}

function abrirHistorico(func: FuncionarioListaDto) {
  funcionarioSelecionado.value = func;
  showModal.value = true;
}

function fecharModal() {
  showModal.value = false;
  funcionarioSelecionado.value = null;
}
</script>

<template>
  <div class="app-layout">
    <AppSidebar />

    <div class="main-wrapper">
      
      <main class="main-content">
        <div class="content-wrapper">
          
          <h1 class="content-title">Área da Psicóloga</h1>

          <div v-if="isLoading" class="loading">Carregando dados...</div>

          <div v-else-if="viewState === 'empresas'">
            <h2>Selecione uma Empresa</h2>
            <div class="grid-cards">
              <div v-for="emp in empresas" :key="emp.id" class="card-empresa" @click="selecionarEmpresa(emp)">
                <h3>{{ emp.nomeEmpresa }}</h3>
                <p>{{ emp.setorAtuacao }}</p>
                <button class="btn-ver">Ver Funcionários</button>
              </div>
            </div>
             <div v-if="empresas.length === 0" class="no-data">Nenhuma empresa encontrada.</div>
          </div>

          <div v-else-if="viewState === 'funcionarios'">
            <button class="btn-voltar-top" @click="voltarParaEmpresas">← Voltar para Empresas</button>
            <h2>Funcionários - {{ selectedEmpresa?.nomeEmpresa }}</h2>
            
            <div class="table-responsive">
              <table class="tabela-funcionarios">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Cargo</th>
                    <th style="text-align: center;">Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="func in funcionarios" :key="func.id">
                    <td>{{ func.nome }}</td>
                    <td>{{ func.cargo || 'Não informado' }}</td>
                    
                    <td style="text-align: center;">
                      <button 
                        v-if="func.questionariosRespondidos && func.questionariosRespondidos.length > 0"
                        class="btn-historico"
                        @click="abrirHistorico(func)"
                      >
                        📂 Ver Histórico ({{ func.questionariosRespondidos.length }})
                      </button>
                      <span v-else class="pendente">Pendente</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            
            <div v-if="funcionarios.length === 0" class="no-data">Nenhum funcionário cadastrado nesta empresa.</div>
          </div>
        </div>
      </main>

      <AppFooter />

    </div>

    <div v-if="showModal && funcionarioSelecionado" class="modal-overlay" @click.self="fecharModal">
      <div class="modal-content">
        <header class="modal-header">
          <h3>Histórico: {{ funcionarioSelecionado.nome }}</h3>
          <button class="btn-close" @click="fecharModal">×</button>
        </header>
        
        <div class="modal-body">
          <p class="cargo-info">Cargo: {{ funcionarioSelecionado.cargo }}</p>
          <hr>
          
          <div class="lista-historico">
            <div v-for="quest in funcionarioSelecionado.questionariosRespondidos" :key="quest.tokenAcesso" class="item-historico">
              <div class="info-quest">
                <span class="titulo">{{ quest.titulo }}</span>
                <span class="data">Respondido em: {{ new Date(quest.dataResposta).toLocaleDateString() }}</span>
              </div>
              <a 
                :href="`${API_URL}/api/questionario/download-pdf/${quest.tokenAcesso}`" 
                target="_blank" 
                class="btn-pdf"
              >
                ⬇ Baixar PDF
              </a>
            </div>
          </div>
        </div>

        <footer class="modal-footer">
          <button class="btn-fechar-modal" @click="fecharModal">Fechar</button>
        </footer>
      </div>
    </div>

  </div>
</template>

<style scoped>
/* RESET */
:global(html), :global(body), :global(#app) {
  height: 100%;
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  overflow: hidden;
}
:global(body) { 
  background-color: #f0f2f5; 
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
}

.app-layout { 
  display: flex; 
  min-height: 100vh; 
  width: 100%;
  flex-direction: row;
}

/* REMOVI TODO O CSS DA SIDEBAR DAQUI
   POIS AGORA ESTÁ DENTRO DE AppSidebar.vue 
*/

/* MAIN WRAPPER */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow-y: auto;
}

/* MAIN CONTENT */
.main-content { 
  flex: 1;
  background-color: #f0f2f5; 
  padding: 2rem; 
  display: flex; 
  justify-content: center; 
  align-items: flex-start; 
}

.content-wrapper { 
  max-width: 1200px; 
  width: 100%; 
  padding: 2.5rem 3rem; 
  border-radius: 8px; 
  background-color: #ffffff; 
  color: #333; 
  box-shadow: 0 4px 6px rgba(0,0,0,0.05); 
  margin-bottom: 2rem;
}

.content-title { font-size: 2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 2rem; display: inline-block; }

/* GRID EMPRESAS */
.grid-cards { 
  display: grid; 
  grid-template-columns: repeat(4, 1fr); 
  gap: 1.5rem; 
}

.card-empresa { 
  background: white; 
  padding: 1.5rem; 
  border-radius: 8px; 
  border: 1px solid #e0e0e0; 
  cursor: pointer; 
  transition: all 0.2s; 
  display: flex; 
  flex-direction: column; 
  justify-content: space-between; 
  min-height: 180px;
}
.card-empresa:hover { transform: translateY(-3px); box-shadow: 0 8px 15px rgba(0,0,0,0.1); border-color: #3b82f6; }
.card-empresa h3 { margin: 0 0 0.5rem 0; color: #1f2937; font-size: 1.1rem; font-weight: 700; }
.card-empresa p { color: #6b7280; margin-bottom: 1.5rem; font-size: 0.9rem; }
.btn-ver { margin-top: auto; width: 100%; padding: 0.6rem; background: #e0eafc; border: none; color: #3b82f6; font-weight: bold; border-radius: 6px; cursor: pointer; transition: background 0.2s; }
.btn-ver:hover { background: #dbeafe; }

/* TABELA */
.table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
.tabela-funcionarios { width: 100%; border-collapse: separate; border-spacing: 0; background-color: #fff; margin-top: 1.5rem; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.05); border: 1px solid #e5e7eb; min-width: 600px; }
.tabela-funcionarios th { background-color: #f9fafb; font-weight: 600; color: #374151; padding: 1rem 1.5rem; text-align: left; border-bottom: 1px solid #e5e7eb; text-transform: uppercase; font-size: 0.8rem; letter-spacing: 0.05em; }
.tabela-funcionarios td { padding: 1rem 1.5rem; border-bottom: 1px solid #e5e7eb; vertical-align: middle; color: #4b5563; }
.pendente { color: #9ca3af; font-style: italic; font-size: 0.9rem; background: #f3f4f6; padding: 0.3rem 0.6rem; border-radius: 4px; }
.btn-historico { background-color: #3b82f6; color: white; border: none; padding: 0.5rem 1rem; border-radius: 6px; cursor: pointer; font-weight: 600; transition: background 0.2s; display: inline-flex; align-items: center; gap: 5px; }
.btn-historico:hover { background-color: #2563eb; }
.btn-voltar-top { margin-bottom: 1rem; background: none; border: none; color: #6b7280; cursor: pointer; font-weight: 600; display: inline-flex; align-items: center; padding: 0; font-size: 0.95rem; }

/* MODAL */
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0, 0, 0, 0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; backdrop-filter: blur(2px); }
.modal-content { background: white; width: 90%; max-width: 600px; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.2); display: flex; flex-direction: column; max-height: 85vh; animation: modalFadeIn 0.3s ease; }
@keyframes modalFadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
.modal-header { padding: 1.5rem; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
.btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #666; }
.modal-body { padding: 1.5rem; overflow-y: auto; }
.lista-historico { display: flex; flex-direction: column; gap: 10px; }
.item-historico { display: flex; justify-content: space-between; align-items: center; background: #f8f9fa; padding: 1rem; border-radius: 8px; border: 1px solid #e9ecef; flex-wrap: wrap; gap: 10px; }
.info-quest { display: flex; flex-direction: column; }
.info-quest .titulo { font-weight: bold; color: #333; margin-bottom: 2px; }
.info-quest .data { font-size: 0.85rem; color: #777; }
.btn-pdf { background-color: #dc3545; color: white; text-decoration: none; padding: 0.5rem 1rem; border-radius: 6px; font-weight: bold; font-size: 0.9rem; transition: background 0.2s; text-align: center; }
.btn-pdf:hover { background-color: #bb2d3b; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #eee; text-align: right; }
.btn-fechar-modal { padding: 0.6rem 1.2rem; background: #e0e0e0; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; color: #333; }

/* RESPONSIVIDADE */
@media (max-width: 1200px) {
  .grid-cards { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 900px) {
  .grid-cards { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .app-layout { flex-direction: column; }
  /* Ajuste no CSS da sidebar que sobrou aqui no componente pai caso precise, 
     mas o ideal é o componente Sidebar cuidar de si mesmo */
  .content-wrapper { padding: 1.5rem; margin-top: 0; border-radius: 0; }
  .main-content { padding: 0; background-color: #f0f2f5; }
  .grid-cards { grid-template-columns: 1fr; }
  .item-historico { flex-direction: column; align-items: flex-start; }
  .btn-pdf { width: 100%; margin-top: 0.5rem; }
  .main-wrapper { height: auto; overflow-y: visible; }
}
</style>