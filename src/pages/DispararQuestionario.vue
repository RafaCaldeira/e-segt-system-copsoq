<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import { useRouter } from 'vue-router';
import type { Empresa } from '../types/empresa.types';
import type { Funcionario } from '../types/funcionario.types';
import type { DisparoCreateDto } from '../types/disparo.types';

// Interface local
interface QuestionarioSimples {
  id: number;
  titulo: string;
  setoresAplicaveis: { setor: string }[]; 
}

const userStore = useUserStore();
const router = useRouter();

function handleLogout() {
  localStorage.removeItem('token') // ou o que você usa
  router.push('/login')
}

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
const selectedSetor = ref<string>(''); // Filtro de Setor
const selectedFuncionariosIds = ref<number[]>([]);
const allSelected = ref(false);


// --- COMPUTED: SETORES DISPONÍVEIS (DINÂMICO) ---
// *** CORREÇÃO AQUI ***
// Em vez de lista fixa, pegamos os setores dos funcionários carregados
const setoresDisponiveis = computed(() => {
  if (funcionarios.value.length === 0) return [];
  
  // Extrai os setores únicos dos funcionários
  const setoresUnicos = new Set(funcionarios.value.map(f => f.setor));
  // Retorna como array ordenado
  return Array.from(setoresUnicos).sort();
});


// --- COMPUTED: Filtragem Inteligente de Questionários ---
const questionariosFiltrados = computed(() => {
  if (!selectedEmpresaId.value) return [];

  const empresa = empresas.value.find(e => e.id === selectedEmpresaId.value);
  const setorEmpresa = empresa?.setorAtuacao || "";

  return questionarios.value.filter(q => {
    // A. Se é Geral (sem restrições), mostra sempre
    if (!q.setoresAplicaveis || q.setoresAplicaveis.length === 0) return true;
    // B. Se tem restrições, verifica se bate com o setor da empresa
    return q.setoresAplicaveis.some((s: any) => {
      const sNome = s.setor || s.Setor; 
      return sNome === setorEmpresa;
    });
  });
});

// --- COMPUTED: Filtragem de Funcionários ---
const funcionariosFiltrados = computed(() => {
  let lista = funcionarios.value;

  // Filtra por Setor (se selecionado no dropdown)
  if (selectedSetor.value) {
    lista = lista.filter(f => f.setor === selectedSetor.value);
  }

  return lista;
});

// --- WATCHERS ---

// Selecionar Todos (só os filtrados)
watch(allSelected, (val) => {
  if (val) {
    selectedFuncionariosIds.value = funcionariosFiltrados.value.map(f => f.id);
  } else {
    selectedFuncionariosIds.value = [];
  }
});

// Carregar funcionários ao mudar a empresa
watch(selectedEmpresaId, async (newId) => {
  selectedFuncionariosIds.value = [];
  selectedQuestionarioId.value = null;
  selectedSetor.value = ''; // Reseta o filtro de setor
  allSelected.value = false;
  funcionarios.value = []; 
  
  if (newId) {
    isLoading.value = true;
    const data = await apiService.getFuncionarios(); 
    if (data) {
        // Filtra localmente para garantir
        funcionarios.value = data.filter((f: any) => {
            const fEmpresaId = f.empresaID || f.EmpresaID || f.empresaId;
            return fEmpresaId === newId;
        });
    }
    isLoading.value = false;
  }
});

// --- LOAD ---
onMounted(async () => {
  if (!userStore.isAdmin) {
    router.push('/dashboard');
    return;
  }

  isLoading.value = true;
  
  const [resEmpresas, resQuestionarios] = await Promise.all([
    apiService.getEmpresas(),
    apiService.getQuestionarios()
  ]);

  if (resEmpresas) empresas.value = resEmpresas;
  if (resQuestionarios) questionarios.value = resQuestionarios as any[];
  
  isLoading.value = false;
});

// --- ENVIAR ---
async function enviarDisparos() {
  if (!selectedQuestionarioId.value) return alert("Selecione um questionário.");
  if (selectedFuncionariosIds.value.length === 0) return alert("Selecione pelo menos um funcionário.");

  isLoading.value = true;
  message.value = null;

  const dto: DisparoCreateDto = {
    questionarioID: selectedQuestionarioId.value,
    funcionarioIDs: selectedFuncionariosIds.value
  };

  const result = await apiService.createDisparo(dto);

  if (result.success) {
    message.value = { text: result.message, type: 'success' };
    selectedFuncionariosIds.value = [];
    allSelected.value = false;
  } else {
    message.value = { text: result.message, type: 'error' };
  }
  isLoading.value = false;
}
</script>

<template>
  <div class="app-layout">
    <nav class="sidebar">
      <img src="../assets/logo-e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon"></span> Administrador</li>
        <li><router-link to="/dashboard"><span class="icon"></span> Dashboard</router-link></li>
        <li class="active"><a href="#"><span class="icon"></span> Disparar Formulários</a></li>
        <li><router-link to="/criar-questionario"><span class="icon"></span> Criar Formulário</router-link></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon icon-logout"></span> Sair</a></li>
      </ul>
    </nav>

    <main class="main-content">
      <div class="responder-container">
        <h1 class="content-title">Disparar Formulários</h1>
        <p class="desc">Selecione a empresa para filtrar os questionários disponíveis.</p>

        <div v-if="message" :class="['alert', message.type === 'success' ? 'alert-success' : 'alert-error']">
          {{ message.text }}
        </div>

        <div class="filters-card">
          <div class="form-grid">
            
            <!-- 1. EMPRESA -->
            <div class="form-group">
              <label>Selecione a Empresa</label>
              <select v-model="selectedEmpresaId" :disabled="isLoading">
                <option :value="null" disabled>-- Escolha uma empresa --</option>
                <option v-for="emp in empresas" :key="emp.id" :value="emp.id">
                  {{ emp.nomeEmpresa }} ({{ emp.setorAtuacao }})
                </option>
              </select>
            </div>

            <!-- 2. QUESTIONÁRIO -->
            <div class="form-group">
              <label>Selecione o Formulário</label>
              <select v-model="selectedQuestionarioId" :disabled="!selectedEmpresaId || isLoading">
                <option :value="null" disabled>
                  {{ selectedEmpresaId ? '-- Escolha o questionário --' : '-- Selecione a empresa primeiro --' }}
                </option>
                <option v-for="q in questionariosFiltrados" :key="q.id" :value="q.id">
                  {{ q.titulo }}
                </option>
              </select>
            </div>

             <!-- 3. FILTRO SETOR (AGORA DINÂMICO) -->
             <div class="form-group">
              <label>Filtrar Funcionários por Setor</label>
              <select v-model="selectedSetor" :disabled="!selectedEmpresaId || isLoading || funcionarios.length === 0">
                <option value="">Todos os Setores</option>
                <!-- Usa o Computed 'setoresDisponiveis' -->
                <option v-for="setor in setoresDisponiveis" :key="setor" :value="setor">
                  {{ setor }}
                </option>
              </select>
            </div>

          </div>
        </div>

        <!-- TABELA DE FUNCIONÁRIOS -->
        <div v-if="selectedEmpresaId" class="selection-area">
          <div class="selection-header">
            <h3>Funcionários ({{ selectedFuncionariosIds.length }} selecionados)</h3>
            <button class="btn-enviar" @click="enviarDisparos" :disabled="isLoading || selectedFuncionariosIds.length === 0">
              {{ isLoading ? 'Enviando...' : '🚀 Enviar Agora' }}
            </button>
          </div>

          <table class="selection-table">
            <thead>
              <tr>
                <th style="width: 40px;">
                  <input type="checkbox" v-model="allSelected">
                </th>
                <th>Nome</th>
                <th>Email</th>
                <th>Setor</th>
                <th>Cargo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="func in funcionariosFiltrados" :key="func.id" :class="{ selected: selectedFuncionariosIds.includes(func.id) }">
                <td>
                  <input type="checkbox" :value="func.id" v-model="selectedFuncionariosIds">
                </td>
                <td>{{ func.nome }}</td>
                <td>{{ func.email }}</td>
                <td><span class="badge">{{ func.setor }}</span></td>
                <td>{{ func.cargo }}</td>
              </tr>
            </tbody>
          </table>

          <div v-if="funcionariosFiltrados.length === 0" class="no-data">
            Nenhum funcionário encontrado para este filtro.
          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* (Mesmos estilos) */
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
.responder-container { max-width: 1000px; width: 100%; padding: 2.5rem 3rem; border-radius: 8px; background-color: #f4f7f6; color: #333; }
.content-title { font-size: 2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 1rem; display: inline-block; }
.desc { color: #666; margin-bottom: 2rem; }

.filters-card { background: #fff; padding: 1.5rem; border-radius: 8px; border: 1px solid #ddd; margin-bottom: 2rem; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1.5rem; }
.form-group { display: flex; flex-direction: column; }
.form-group label { font-weight: bold; margin-bottom: 0.5rem; color: #555; font-size: 0.9rem; }
.form-group select { padding: 0.8rem; border: 1px solid #ccc; border-radius: 4px; font-size: 1rem; background-color: #fff; color: #333; }

.selection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.btn-enviar { padding: 0.8rem 1.5rem; background-color: #28a745; color: white; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; font-size: 1rem; transition: background 0.2s; }
.btn-enviar:hover:not(:disabled) { background-color: #218838; }
.btn-enviar:disabled { background-color: #aaa; cursor: not-allowed; }

.selection-table { width: 100%; border-collapse: collapse; background-color: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.selection-table th, .selection-table td { padding: 1rem; text-align: left; border-bottom: 1px solid #eee; }
.selection-table th { background-color: #f8f9fa; font-weight: bold; color: #555; text-transform: uppercase; font-size: 0.85rem; }
.selection-table tr:hover { background-color: #f1f1f1; }
.selection-table tr.selected { background-color: #e0eafc; }
.badge { background-color: #eee; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.85rem; color: #555; }

.alert { padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; font-weight: bold; }
.alert-success { background-color: #d1e7dd; color: #0f5132; border: 1px solid #badbcc; }
.alert-error { background-color: #f8d7da; color: #842029; border: 1px solid #f5c6cb; }
.no-data-message { text-align: center; padding: 3rem; font-size: 1.2rem; color: #777; border: 2px dashed #ccc; border-radius: 8px; }
.no-data { text-align: center; padding: 2rem; color: #888; }
</style>