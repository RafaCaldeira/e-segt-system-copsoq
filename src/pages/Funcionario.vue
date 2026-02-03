<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import { useRouter } from 'vue-router';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

const userStore = useUserStore();
const router = useRouter();

// Estado
const funcionarios = ref<Funcionario[]>([]);
const isLoading = ref(true);
const isImporting = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);
const importErrors = ref<string[]>([]);

// Pesquisa e filtro
const search = ref('');
const setorFiltro = ref('');

// --- Carregamento Inicial ---
onMounted(async () => {
  if (!userStore.isLoggedIn) { 
    router.push('/login');
    return;
  }
  await carregarFuncionarios();
});

async function carregarFuncionarios() {
  isLoading.value = true;
  try {
    const data = await apiService.getFuncionarios();
    if (data) {
      funcionarios.value = data;
    } else {
      errorMessage.value = "Não foi possível carregar a lista de funcionários.";
    }
  } catch (error) {
    errorMessage.value = "Erro de conexão ao buscar funcionários.";
  } finally {
    isLoading.value = false;
  }
}

// --- Computeds: Filtros ---
const funcionariosFiltrados = computed(() => {
  const q = search.value.trim().toLowerCase();
  return funcionarios.value.filter(f => {
    const matchesSearch =
      !q ||
      f.nome.toLowerCase().includes(q) ||
      (f.cargo && f.cargo.toLowerCase().includes(q)) ||
      (f.setor && f.setor.toLowerCase().includes(q));
    const matchesSetor = !setorFiltro.value || f.setor === setorFiltro.value;
    return matchesSearch && matchesSetor;
  });
});

const setoresUnicos = computed(() => {
  const s = funcionarios.value.map(f => f.setor || '');
  return Array.from(new Set(s)).filter(x => x !== '');
});

// --- Importar CSV ---
async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  if (!target.files || target.files.length === 0) return;

  const file = target.files?.[0];
  if (!file) return;

  if (!file.name.endsWith('.csv')) {
    alert('Por favor, selecione um arquivo .csv');
    return;
  }

  isImporting.value = true;
  errorMessage.value = null;
  successMessage.value = null;
  importErrors.value = [];

  try {
    const result = await apiService.importarFuncionariosCsv(file);

    // Agora tratamos o caso de sucesso ou erro baseado na estrutura do JSON
    if (result && result.success) {
      successMessage.value = result.message ?? 'Importação concluída com sucesso.';
      if (result.erros && result.erros.length > 0) {
        importErrors.value = result.erros;
      }
      await carregarFuncionarios();
    } else {
      // Caso o backend retorne um JSON de erro esperado { success: false, message: "..." }
      errorMessage.value = result?.message ?? 'Falha na importação do arquivo.';
    }
  } catch (e: any) {
    // AQUI É A CHAVE: Capturamos o erro de parsing ou de rede
    console.error("Erro detalhado:", e);
    
    if (e.message.includes("Unexpected token 'E'")) {
      errorMessage.value = "O servidor retornou um erro inesperado (provavelmente uma falha no código do backend). Verifique o console ou o log do servidor.";
    } else {
      errorMessage.value = "Erro ao enviar arquivo para o servidor.";
    }
  } finally {
    isImporting.value = false;
    target.value = ''; 
  }
}

// --- Navegação ---
function irParaCadastroManual() {
  router.push('/novo-funcionario'); 
}

function editarFuncionario(id: number) {
  // Ajuste conforme sua rota definida no router/index.ts
  router.push({ name: 'EditarFuncionario', params: { id: id } });
}

async function excluirFuncionario(id: number) {
  if (!confirm('Tem certeza que deseja remover este colaborador?')) return;

  try {
    const res = await apiService.deleteFuncionario(id);
    if (res) {
      successMessage.value = "Funcionário removido.";
      await carregarFuncionarios();
    } else {
      errorMessage.value = "Erro ao remover funcionário.";
    }
  } catch (err) {
    errorMessage.value = 'Erro de conexão ao excluir.';
  }
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
              <h1 class="content-title">Gestão de Colaboradores</h1>
              <p class="subtitle">Adicione, edite ou importe a lista de funcionários para as avaliações.</p>
            </div>
            
            <button @click="irParaCadastroManual" class="btn-primary">
              + Novo Funcionário
            </button>
          </header>

          <div v-if="successMessage" class="alert success fade-in">
            ✅ {{ successMessage }}
            <ul v-if="importErrors.length > 0" class="warning-list">
              <li v-for="err in importErrors" :key="err">{{ err }}</li>
            </ul>
          </div>
          <div v-if="errorMessage" class="alert error fade-in">
            ⚠️ {{ errorMessage }}
          </div>

          <div class="toolbar">
            <div class="filters">
              <div class="search-box">
                <span class="search-icon">🔍</span>
                <input v-model="search" placeholder="Buscar por nome, cargo..." class="search-input" />
              </div>

              <select v-model="setorFiltro" class="select-filter">
                <option value="">Todos os Setores</option>
                <option v-for="s in setoresUnicos" :key="s" :value="s">{{ s }}</option>
              </select>
            </div>

            <div class="import-actions">
              <input type="file" id="csvUpload" @change="handleFileUpload" accept=".csv" :disabled="isImporting" hidden>
              <label for="csvUpload" class="btn-outline" :class="{ 'disabled': isImporting }">
                {{ isImporting ? '⏳ Importando...' : '📂 Importar CSV' }}
              </label>
            </div>
          </div>

          <div v-if="isLoading" class="loading-state">
            <div class="spinner"></div> Carregando lista...
          </div>

          <div v-else class="table-card">
            <div class="table-responsive">
              <table class="styled-table">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Email</th>
                    <th>Cargo</th>
                    <th>Setor</th>
                    <th>CPF</th>
                    <th class="text-center">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="func in funcionariosFiltrados" :key="func.id">
                    <td class="fw-bold">{{ func.nome }}</td>
                    <td>{{ func.email }}</td>
                    <td>{{ func.cargo }}</td>
                    <td><span class="badge">{{ func.setor }}</span></td>
                    <td class="mono">{{ func.cpf || '-' }}</td>
                    <td class="text-center actions-cell">
                      <button class="btn-icon edit" @click="editarFuncionario(func.id)" title="Editar">✏️</button>
                      <button class="btn-icon delete" @click="excluirFuncionario(func.id)" title="Excluir">🗑️</button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div v-if="funcionariosFiltrados.length === 0" class="empty-state">
              <p>Nenhum funcionário encontrado.</p>
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

/* --- MAIN WRAPPER --- */
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

.content-wrapper { 
  max-width: 1100px; 
  width: 100%; 
  background: white; 
  padding: 2.5rem;
  border-radius: 12px; 
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); 
  margin-bottom: 2rem;
}

/* Header */
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; border-bottom: 1px solid #eee; padding-bottom: 1rem; flex-wrap: wrap; gap: 1rem; }
.content-title { font-size: 1.8rem; color: #1f2937; margin: 0; }
.subtitle { color: #6b7280; margin: 5px 0 0 0; }

.btn-primary { padding: 0.75rem 1.5rem; background: #2563eb; color: white; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2); }
.btn-primary:hover { background: #1d4ed8; transform: translateY(-1px); }

/* Toolbar */
.toolbar {
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  background: #f8fafc; 
  padding: 1.2rem; 
  border-radius: 8px; 
  border: 1px solid #e2e8f0; 
  margin-bottom: 1.5rem; 
  flex-wrap: wrap; 
  gap: 1.5rem; 
}

.filters { 
  display: flex; 
  gap: 15px; 
  flex: 1; 
  min-width: 300px;
  align-items: center;
  flex-wrap: wrap;
}

.search-box { position: relative; flex-grow: 1; min-width: 200px; }
.search-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #94a3b8; }

.search-input { 
  width: 100%; 
  padding: 0.7rem 0.7rem 0.7rem 2.5rem; 
  border: 1px solid #cbd5e1; 
  border-radius: 6px; 
  font-size: 0.95rem; 
  transition: border-color 0.2s; 
  box-sizing: border-box;
}
.search-input:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }

.select-filter { 
  padding: 0.7rem; 
  border: 1px solid #cbd5e1; 
  border-radius: 6px; 
  background: white; 
  cursor: pointer; 
  min-width: 180px; 
  font-size: 0.95rem; 
}

.import-actions { display: flex; align-items: center; gap: 1rem; }
.btn-outline { padding: 0.7rem 1.2rem; border: 1px solid #cbd5e1; color: #475569; background: white; border-radius: 6px; cursor: pointer; font-weight: 600; font-size: 0.9rem; transition: all 0.2s; display: inline-flex; align-items: center; }
.btn-outline:hover { background: #f1f5f9; border-color: #94a3b8; color: #1e293b; }
.btn-outline.disabled { opacity: 0.6; cursor: wait; }

/* Table */
.table-card { border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; }
.table-responsive { overflow-x: auto; }
.styled-table { width: 100%; border-collapse: collapse; font-size: 0.95rem; }
.styled-table thead tr { background-color: #f8fafc; text-align: left; }
.styled-table th { padding: 1rem 1.2rem; font-weight: 600; color: #475569; text-transform: uppercase; font-size: 0.8rem; letter-spacing: 0.05em; border-bottom: 1px solid #e2e8f0; }
.styled-table td { padding: 1rem 1.2rem; border-bottom: 1px solid #f1f5f9; color: #334155; vertical-align: middle; }
.styled-table tbody tr:last-child td { border-bottom: none; }
.styled-table tbody tr:hover { background-color: #f8fafc; }

.badge { background: #e0f2fe; color: #0284c7; padding: 4px 10px; border-radius: 20px; font-size: 0.8rem; font-weight: 600; border: 1px solid #bae6fd; }
.fw-bold { font-weight: 600; color: #1e293b; }
.mono { font-family: 'Consolas', monospace; color: #64748b; font-size: 0.9rem; }
.text-center { text-align: center; }

.actions-cell { white-space: nowrap; }
.btn-icon { background: none; border: none; font-size: 1.1rem; cursor: pointer; padding: 6px; transition: transform 0.2s, background 0.2s; border-radius: 4px; }
.btn-icon:hover { transform: scale(1.1); background: #f1f5f9; }
.edit { color: #f59e0b; }
.delete { color: #ef4444; }

/* Feedback */
.alert { padding: 1rem; border-radius: 8px; margin-bottom: 1.5rem; display: flex; flex-direction: column; gap: 0.5rem; }
.alert.success { background: #ecfdf5; color: #065f46; border: 1px solid #a7f3d0; }
.alert.error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
.warning-list { margin: 0; padding-left: 1.2rem; font-size: 0.9rem; color: #92400e; }

.loading-state, .empty-state { text-align: center; padding: 4rem; color: #64748b; background: #f9fafb; border-radius: 8px; border: 1px dashed #e2e8f0; margin-top: 1rem; }
.spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #e2e8f0; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-right: 10px; vertical-align: middle; }
@keyframes spin { to { transform: rotate(360deg); } }

.fade-in { animation: fadeIn 0.3s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .filters { flex-direction: column; align-items: stretch; }
  .toolbar { flex-direction: column; align-items: stretch; }
}
</style>