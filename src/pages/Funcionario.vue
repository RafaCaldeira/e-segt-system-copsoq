<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import { useRouter } from 'vue-router';

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
const setorFiltro = ref(''); // "" = todos

// Estado do Modal de Cadastro Manual (Simplificado)
//const showModal = ref(false);
//const novoFuncionario = ref({ nome: '', email: '', cargo: '', setor: '', cpf: '', telefone: '' });

// --- Lógica da Sidebar ---
function handleLogout() { userStore.logout(); router.push('/login'); }
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole);

// --- Carregamento Inicial ---
onMounted(async () => {
  if (!userStore.isLoggedIn || !userStore.isCliente) {
    router.push('/login');
    return;
  }
  await carregarFuncionarios();
});

async function carregarFuncionarios() {
  isLoading.value = true;
  const data = await apiService.getFuncionarios();
  if (data) {
    funcionarios.value = data;
  } else {
    errorMessage.value = "Erro ao carregar lista.";
  }
  isLoading.value = false;
}

// --- Computeds: lista filtrada e lista de setores ---
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

  const result = await apiService.importarFuncionariosCsv(file);

  if (result?.success) {
    successMessage.value = result.message ?? 'Importação concluída.';
    if (result.erros && result.erros.length > 0) {
      importErrors.value = result.erros;
    }
    await carregarFuncionarios();
  } else {
    errorMessage.value = result?.message ?? 'Erro na importação.';
  }

  isImporting.value = false;
  // limpa input para permitir re-upload do mesmo arquivo
  target.value = '';
}

// --- Download Modelo CSV ---
function baixarModeloCsv() {
  const csvContent = "Nome;Email;Telefone;Cargo;Setor;CPF\nJoao Silva;joao@email.com;1199999999;Operador;Producao;12345678900";
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement("a");
  const url = URL.createObjectURL(blob);
  link.href = url;
  link.download = "modelo_funcionarios.csv";
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

// --- Navegação para criação/edição ---
function irParaCadastroManual() {
  // caminho sugerido: /funcionarios/novo
  router.push('/novo-funcionario');
}

function editarFuncionario(id: number) {
  // caminho sugerido: /funcionarios/editar/:id
  router.push({ name: 'EditarFuncionario', params: { id: id } });
}

// --- Excluir funcionário ---
async function excluirFuncionario(id: number) {
  const ok = confirm('Deseja realmente excluir este funcionário?');
  if (!ok) return;

  try {
    const res = await apiService.deleteFuncionario(id);

    if (ok) {
      alert("Funcionário deletado com sucesso!");
      carregarFuncionarios(); // se existir
    } else {
      alert("Erro ao deletar funcionário.");
    }

  } catch (err) {
      alert('Erro ao excluir funcionário.');
  }
}
</script>

<template>
  <div class="app-layout">
    <nav class="sidebar">
      <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon"></span> {{ displayName }}</li>
        <li class="active"><a href="#"><span class="icon"></span> Editar Cadastro</a></li>
        <li><router-link to="/plano-de-acao"><span class="icon"></span> Plano de ação</router-link></li>
        <li><router-link to="/relatorio"><span class="icon"></span> Relatórios</router-link></li>
        <li><a href="#"><span class="icon"></span> Baixar Roadmap</a></li>
        <li><a href="#"><span class="icon"></span> Histórico</a></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon icon-logout"></span> Sair</a></li>
      </ul>
    </nav>

    <main class="main-content">
      <div class="responder-container">
        <h1 class="content-title">Gestão de Funcionários</h1>
        <p>Gerencie os colaboradores que participarão nas avaliações.</p>

        <!-- Área de Ações (Importar, Filtro, Adicionar) -->
        <div class="actions-bar">
          <div class="left-actions">
            <div class="import-wrapper">
              <input type="file" id="csvUpload" @change="handleFileUpload" accept=".csv" :disabled="isImporting" hidden>
              <label for="csvUpload" class="btn-secondary" :class="{ 'disabled': isImporting }">
                <span v-if="isImporting">A importar...</span>
                <span v-else>📂 Importar CSV</span>
              </label>
              <button @click="baixarModeloCsv" class="btn-link-small">Baixar Modelo</button>
            </div>

            <!-- Barra de pesquisa -->
            <div class="search-wrapper">
              <input v-model="search" placeholder="Pesquisar por nome, cargo ou setor..." class="search-input" />
            </div>

            <!-- Seletor de setor -->
            <div class="filter-wrapper">
              <select v-model="setorFiltro" class="select-setor">
                <option value="">Todos os setores</option>
                <option v-for="s in setoresUnicos" :key="s" :value="s">{{ s }}</option>
              </select>
            </div>
          </div>

          <div class="right-actions">
            <button @click="irParaCadastroManual" class="btn-continuar">+ Novo Funcionário</button>
          </div>
        </div>

        <!-- Feedback de Sucesso/Erro -->
        <div v-if="successMessage" class="success-message">
          {{ successMessage }}
          <ul v-if="importErrors.length > 0" class="warning-list">
            <li v-for="err in importErrors" :key="err">{{ err }}</li>
          </ul>
        </div>
        <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>

        <!-- Tabela de Funcionários -->
        <div v-if="isLoading" class="loading">Carregando...</div>

        <table v-else class="funcionarios-tabela">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Email</th>
              <th>Cargo</th>
              <th>Setor</th>
              <th>CPF</th>
              <th style="width: 160px">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="func in funcionariosFiltrados" :key="func.id">
              <td>{{ func.nome }}</td>
              <td>{{ func.email }}</td>
              <td>{{ func.cargo }}</td>
              <td>{{ func.setor }}</td>
              <td>{{ func.cpf || '-' }}</td>
              <td>
                <button class="btn-acao" @click="editarFuncionario(func.id)">Editar</button>
                <button class="btn-acao btn-perigo" @click="excluirFuncionario(func.id)">Excluir</button>
              </td>
            </tr>
          </tbody>
        </table>

        <div v-if="!isLoading && funcionariosFiltrados.length === 0" class="no-data">
          Nenhum funcionário encontrado. Ajuste a pesquisa / filtro ou importe um CSV.
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
:global(body) { margin: 0; background-color: #f0f2f5; font-family: Arial, sans-serif; }
.app-layout { display: flex; min-height: 100vh; }
.sidebar { width: 280px; flex-shrink: 0; background-color: #ffffff; padding: 2rem 1.5rem; border-right: 1px solid #e0e0e0; }
.sidebar-logo { width: 150px; margin-bottom: 2rem; display: block; margin: 0 auto 2rem auto; }
.sidebar-nav { list-style: none; padding: 0; margin: 0; }
.sidebar-nav li { margin-bottom: 0.5rem; }
.sidebar-nav li.user-display { font-weight: bold; padding: 1rem; border-bottom: 1px solid #eee; display: flex; align-items: center; color: #333; }
.sidebar-nav a { display: flex; align-items: center; padding: 0.8rem 1rem; border-radius: 6px; text-decoration: none; color: #555; transition: background 0.2s; }
.sidebar-nav a:hover { background-color: #f0f2f5; }
.sidebar-nav li.active a { background-color: #e0eafc; color: #3b82f6; font-weight: bold; }
.sidebar-nav .icon { width: 20px; height: 20px; margin-right: 0.8rem; background-color: #ccc; border-radius: 50%; }
.logout-item { margin-top: 2rem; }
.logout-item a { color: #d9534f; font-weight: bold; }

.main-content { flex: 1; background-color: #333; padding: 2rem; display: flex; justify-content: center; align-items: flex-start; overflow-y: auto; }
.responder-container { max-width: 1000px; width: 100%; padding: 2.5rem 3rem; border-radius: 8px; background-color: #f4f7f6; color: #333; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.content-title { font-size: 2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 1.5rem; display: inline-block; }

/* Ações */
.actions-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; background-color: #fff; padding: 0.75rem; border-radius: 8px; border: 1px solid #ddd; gap: 1rem; }
.left-actions { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.right-actions { display: flex; align-items: center; gap: 0.5rem; }

/* Import */
.import-wrapper { display: flex; align-items: center; gap: 0.5rem; }

/* Search / Filter */
.search-input { padding: 0.5rem 0.75rem; border-radius: 6px; border: 1px solid #ddd; min-width: 260px; }
.select-setor { padding: 0.5rem 0.75rem; border-radius: 6px; border: 1px solid #ddd; }

/* Botões */
.btn-continuar { padding: 0.6rem 1rem; cursor: pointer; border: none; border-radius: 6px; font-weight: bold; background-color: #3b82f6; color: white; transition: background 0.2s; }
.btn-continuar:hover { background-color: #2563eb; }
.btn-secondary { padding: 0.5rem 0.9rem; cursor: pointer; border: 1px solid #3b82f6; border-radius: 6px; font-weight: bold; background-color: white; color: #3b82f6; display: inline-block; transition: background 0.2s; }
.btn-secondary:hover { background-color: #e0eafc; }
.btn-secondary.disabled { opacity: 0.6; cursor: not-allowed; }
.btn-link-small { background: none; border: none; color: #666; text-decoration: underline; cursor: pointer; font-size: 0.9rem; }

/* Mensagens */
.success-message { background-color: #d1e7dd; color: #0f5132; padding: 1rem; border-radius: 6px; margin-bottom: 1rem; }
.error-message { background-color: #f8d7da; color: #842029; padding: 1rem; border-radius: 6px; margin-bottom: 1rem; }
.warning-list { margin-top: 0.5rem; font-size: 0.9rem; color: #664d03; }

/* Tabela */
.funcionarios-tabela { width: 100%; border-collapse: collapse; margin-top: 0.5rem; background-color: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.funcionarios-tabela th, .funcionarios-tabela td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; vertical-align: middle; }
.funcionarios-tabela th { background-color: #f8f9fa; font-weight: bold; color: #555; text-transform: uppercase; font-size: 0.8rem; }
.btn-acao { padding: 0.35rem 0.7rem; margin-right: 0.5rem; border: none; border-radius: 4px; cursor: pointer; background-color: #e0eafc; color: #3b82f6; font-weight: bold; font-size: 0.85rem; }
.btn-perigo { background-color: #fee2e2; color: #dc2626; }
.no-data { text-align: center; padding: 2.5rem; color: #888; font-style: italic; }
</style>
