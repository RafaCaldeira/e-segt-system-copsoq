<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import { useRouter } from 'vue-router';
import type { Empresa } from '../types/empresa.types';
import type { Funcionario } from '../types/funcionario.types';
import type { DisparoCreateDto } from '../types/disparo.types';
// 1. IMPORTAR O FOOTER
import AppFooter from '../components/AppFooter.vue';

// --- Interfaces Locais ---
interface SetorObjeto {
  setor: string;
}

interface QuestionarioSimples {
  id: number;
  titulo: string;
  setoresAplicaveis: SetorObjeto[]; 
}

// --- Config ---
const userStore = useUserStore();
const router = useRouter();

// --- Estado ---
const isLoading = ref(false);
const message = ref<{ text: string; type: 'success' | 'error' } | null>(null);

// Dados
const empresas = ref<Empresa[]>([]);
const questionarios = ref<QuestionarioSimples[]>([]);
const funcionarios = ref<Funcionario[]>([]);

// Seleções
const selectedEmpresaId = ref<number | null>(null);
const selectedQuestionarioId = ref<number | null>(null);
const selectedSetor = ref<string>(''); 
const selectedFuncionariosIds = ref<number[]>([]);
const allSelected = ref(false);

// Permissões
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole);

// --- Logout ---
function handleLogout() {
  userStore.logout();
  router.push('/login');
}

// --- COMPUTED ---
const setoresDisponiveis = computed(() => {
  if (funcionarios.value.length === 0) return [];
  const setoresUnicos = new Set(funcionarios.value.map(f => f.setor));
  return Array.from(setoresUnicos).sort();
});

const questionariosFiltrados = computed(() => {
  if (!selectedEmpresaId.value) return [];
  const empresa = empresas.value.find(e => e.id === selectedEmpresaId.value);
  const setorEmpresa = empresa?.setorAtuacao || "";

  return questionarios.value.filter(q => {
    if (!q.setoresAplicaveis || q.setoresAplicaveis.length === 0) return true;
    return q.setoresAplicaveis.some(s => s.setor === setorEmpresa);
  });
});

const funcionariosFiltrados = computed(() => {
  let lista = funcionarios.value;
  if (selectedSetor.value) {
    lista = lista.filter(f => f.setor === selectedSetor.value);
  }
  return lista;
});

// --- WATCHERS ---
watch(allSelected, (val) => {
  if (val) {
    selectedFuncionariosIds.value = funcionariosFiltrados.value.map(f => f.id);
  } else {
    selectedFuncionariosIds.value = [];
  }
});

watch(selectedEmpresaId, async (newId) => {
  selectedFuncionariosIds.value = [];
  selectedQuestionarioId.value = null;
  selectedSetor.value = '';
  allSelected.value = false;
  funcionarios.value = []; 
  message.value = null;
  
  if (newId) {
    isLoading.value = true;
    try {
      const data = await apiService.getFuncionarios(); 
      if (data) {
        funcionarios.value = data.filter((f: any) => {
           const fEmpresaId = f.empresaID || f.EmpresaID || f.empresaId;
           return Number(fEmpresaId) === Number(newId);
        });
      }
    } catch (e) {
      console.error(e);
      message.value = { text: "Erro ao buscar funcionários.", type: 'error' };
    } finally {
      isLoading.value = false;
    }
  }
});

// --- FUNÇÃO NOVA: Alternar seleção ao clicar na linha ---
function toggleSelection(id: number) {
  const index = selectedFuncionariosIds.value.indexOf(id);
  if (index > -1) {
    selectedFuncionariosIds.value.splice(index, 1); 
  } else {
    selectedFuncionariosIds.value.push(id); 
  }
}

// --- LOAD INICIAL ---
onMounted(async () => {
  // Apenas Admin pode acessar esta tela
  if (!userStore.isAdmin) {
    router.push('/dashboard');
    return;
  }

  isLoading.value = true;
  try {
    const [resEmpresas, resQuestionarios] = await Promise.all([
      apiService.getEmpresas(),
      apiService.getQuestionarios()
    ]);

    if (resEmpresas) empresas.value = resEmpresas;
    if (resQuestionarios) questionarios.value = resQuestionarios as any[];
  } catch (e) {
    message.value = { text: "Erro ao carregar dados iniciais.", type: 'error' };
  } finally {
    isLoading.value = false;
  }
});

// --- AÇÃO DE ENVIAR ---
async function enviarDisparos() {
  if (!selectedQuestionarioId.value) return alert("Selecione um questionário.");
  if (selectedFuncionariosIds.value.length === 0) return alert("Selecione pelo menos um funcionário.");

  if(!confirm(`Confirma o envio para ${selectedFuncionariosIds.value.length} funcionários?`)) return;

  isLoading.value = true;
  message.value = null;

  const dto: DisparoCreateDto = {
    questionarioID: selectedQuestionarioId.value,
    funcionarioIDs: selectedFuncionariosIds.value
  };

  try {
    const result = await apiService.createDisparo(dto);
    if (result && result.success) {
      message.value = { text: 'Disparos enviados com sucesso!', type: 'success' };
      selectedFuncionariosIds.value = [];
      allSelected.value = false;
    } else {
      message.value = { text: result?.message || 'Erro ao enviar.', type: 'error' };
    }
  } catch (e) {
    message.value = { text: 'Erro de comunicação com o servidor.', type: 'error' };
  } finally {
    isLoading.value = false;
  }
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
        <li v-if="userStore.isAdmin" class="active">
          <router-link to="/disparo"><span class="icon">📨</span> Enviar Questionário</router-link>
        </li>

        <li v-if="userStore.isCliente">
            <router-link to="/editar-cadastro"><span class="icon">⚙️</span> Editar Cadastro</router-link>
        </li>
        <li v-if="userStore.isCliente">
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
        <div class="content-wrapper">
          
          <h1 class="page-title">Disparar Formulários</h1>
          <p class="page-desc">Selecione a empresa e o questionário para enviar aos colaboradores.</p>

          <div v-if="message" :class="['alert', message.type === 'success' ? 'alert-success' : 'alert-error']">
            {{ message.text }}
          </div>

          <div class="filters-card">
            <div class="form-grid">
              
              <div class="form-group">
                <label>1. Selecione a Empresa</label>
                <select v-model="selectedEmpresaId" :disabled="isLoading">
                  <option :value="null" disabled>-- Escolha uma empresa --</option>
                  <option v-for="emp in empresas" :key="emp.id" :value="emp.id">
                    {{ emp.nomeEmpresa }}
                  </option>
                </select>
              </div>

              <div class="form-group">
                <label>2. Selecione o Formulário</label>
                <select v-model="selectedQuestionarioId" :disabled="!selectedEmpresaId || isLoading">
                  <option :value="null" disabled>
                    {{ selectedEmpresaId ? '-- Escolha o questionário --' : 'Aguardando Empresa...' }}
                  </option>
                  <option v-for="q in questionariosFiltrados" :key="q.id" :value="q.id">
                    {{ q.titulo }}
                  </option>
                </select>
              </div>

               <div class="form-group">
                <label>3. Filtrar por Setor (Opcional)</label>
                <select v-model="selectedSetor" :disabled="!selectedEmpresaId || isLoading || funcionarios.length === 0">
                  <option value="">Todos os Setores</option>
                  <option v-for="setor in setoresDisponiveis" :key="setor" :value="setor">
                    {{ setor }}
                  </option>
                </select>
              </div>

            </div>
          </div>

          <div v-if="selectedEmpresaId" class="selection-area fade-in">
            
            <div class="selection-header">
              <div class="info-selecao">
                <h3>Funcionários</h3>
                <span class="badge-count">{{ selectedFuncionariosIds.length }} selecionados</span>
              </div>
              
              <button class="btn-enviar" @click="enviarDisparos" :disabled="isLoading || selectedFuncionariosIds.length === 0">
                {{ isLoading ? 'Enviando...' : '🚀 Enviar Disparos' }}
              </button>
            </div>

            <div class="table-container">
              <table class="selection-table">
                <thead>
                  <tr>
                    <th style="width: 50px; text-align: center;">
                      <input type="checkbox" v-model="allSelected" title="Selecionar Todos Visíveis">
                    </th>
                    <th>Nome</th>
                    <th>Email</th>
                    <th>Setor</th>
                    <th>Cargo</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="func in funcionariosFiltrados" :key="func.id" 
                      :class="{ 'row-selected': selectedFuncionariosIds.includes(func.id) }"
                      @click="toggleSelection(func.id)"
                      style="cursor: pointer;">
                    
                    <td style="text-align: center;" @click.stop>
                      <input type="checkbox" :value="func.id" v-model="selectedFuncionariosIds">
                    </td>
                    <td class="fw-bold">{{ func.nome }}</td>
                    <td>{{ func.email }}</td>
                    <td><span class="badge-setor">{{ func.setor }}</span></td>
                    <td>{{ func.cargo }}</td>
                  </tr>
                </tbody>
              </table>

              <div v-if="funcionariosFiltrados.length === 0" class="no-data">
                Nenhum funcionário encontrado com os filtros atuais.
              </div>
            </div>

          </div>

          <div v-else class="empty-state">
            <p>👆 Selecione uma empresa acima para carregar a lista de funcionários.</p>
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

.content-wrapper { max-width: 1000px; width: 100%; background-color: #ffffff; padding: 2.5rem; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 2rem; }
.page-title { font-size: 1.8rem; color: #1f2937; margin: 0 0 0.5rem 0; }
.page-desc { color: #6b7280; margin-bottom: 2rem; }

/* FILTROS */
.filters-card { background: #f9fafb; padding: 1.5rem; border-radius: 8px; border: 1px solid #e5e7eb; margin-bottom: 2rem; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 1.5rem; }
.form-group label { display: block; font-weight: 600; font-size: 0.9rem; margin-bottom: 0.5rem; color: #374151; }
.form-group select { width: 100%; padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 6px; background: white; font-size: 0.95rem; cursor: pointer; }
.form-group select:disabled { background: #e5e7eb; cursor: not-allowed; color: #9ca3af; }

/* SELEÇÃO */
.selection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; flex-wrap: wrap; gap: 1rem; }
.info-selecao h3 { margin: 0; font-size: 1.2rem; color: #111; display: inline-block; margin-right: 10px; }
.badge-count { background: #dbeafe; color: #1e40af; padding: 2px 8px; border-radius: 12px; font-size: 0.85rem; font-weight: bold; }
.btn-enviar { padding: 0.75rem 1.5rem; background-color: #10b981; color: white; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; transition: background 0.2s; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.btn-enviar:hover:not(:disabled) { background-color: #059669; }
.btn-enviar:disabled { background-color: #9ca3af; cursor: not-allowed; box-shadow: none; }

/* TABELA */
.table-container { border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden; }
.selection-table { width: 100%; border-collapse: collapse; }
.selection-table th { background: #f3f4f6; padding: 1rem; text-align: left; color: #4b5563; font-weight: 600; border-bottom: 1px solid #e5e7eb; }
.selection-table td { padding: 0.8rem 1rem; border-bottom: 1px solid #f3f4f6; color: #374151; }
.selection-table tr:hover { background-color: #f9fafb; }
.selection-table tr.row-selected { background-color: #eff6ff; }
.badge-setor { background: #f3f4f6; padding: 4px 8px; border-radius: 4px; font-size: 0.85rem; color: #4b5563; border: 1px solid #e5e7eb; }
.fw-bold { font-weight: 600; }

/* UTIL */
.alert { padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; font-weight: 500; }
.alert-success { background-color: #d1fae5; color: #065f46; border: 1px solid #a7f3d0; }
.alert-error { background-color: #fee2e2; color: #991b1b; border: 1px solid #fecaca; }
.no-data { text-align: center; padding: 2rem; color: #6b7280; font-style: italic; }
.empty-state { text-align: center; color: #9ca3af; margin-top: 3rem; font-size: 1.1rem; }
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
}
</style>