<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types';
import { useRouter } from 'vue-router';
// 1. IMPORTAR O FOOTER
import AppFooter from '../components/AppFooter.vue';

// Estado
const funcionarios = ref<Funcionario[] | null>(null);
const empresas = ref<Empresa[] | null>(null); 
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- Lógica da Sidebar ---
function handleLogout() {
  userStore.logout();
  router.push('/login');
}

// Computa o nome/cargo para exibir no topo do menu
const displayName = computed(() => {
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  
  if (userStore.isCliente && userStore.nomeEmpresa) {
    return userStore.nomeEmpresa; 
  }
  if (userStore.isCliente) return "Cliente"; 

  return "Menu";
});

// --- NOVA FUNÇÃO: Gerir Empresa ---
function gerirEmpresa(empresaId: number) {
  router.push({ 
    path: '/historico', 
    query: { empresaId: empresaId.toString() } 
  });
}

// --- Lógica do Conteúdo Principal (Dashboard) ---
onMounted(async () => {
  if (!userStore.isLoggedIn) {
    errorMessage.value = "Acesso negado. Por favor, faça o login.";
    isLoading.value = false;
    router.push('/login');
    return;
  }
  
  isLoading.value = true;

  try {
    if (userStore.isCliente) {
      const data = await apiService.getFuncionarios();
      funcionarios.value = data || [];
    } 
    else if (userStore.isAdmin) {
      const data = await apiService.getEmpresas();
      empresas.value = data || [];
    } else if (userStore.isAdmin || userStore.userRole === 'Psicologo') {
      const data = await apiService.getEmpresasParaPsicologo(); 
      empresas.value = data || [];
    }
  } catch (error) {
    errorMessage.value = "Erro ao carregar dados.";
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="app-layout">
    
    <nav class="sidebar">
      <div class="logo-area">
        <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      </div>
      
      <div class="user-badge">{{ displayName }}</div>
      
      <ul class="sidebar-nav">
        
        <li v-if="userStore.userRole === 'Psicologo'">
            <router-link to="/psicologo"><span class="icon">🧠</span> Área do Psicólogo</router-link>
        </li>

        <li v-if="userStore.isAdmin">
          <router-link to="/criar-questionario">
            <span class="icon">📝</span> Criar Questionário
          </router-link>
        </li>
        <li v-if="userStore.isAdmin">
          <router-link to="/disparo">
            <span class="icon">📨</span> Enviar Questionário
          </router-link>
        </li>

        <li v-if="userStore.isCliente">
            <router-link to="/editar-cadastro">
                <span class="icon">⚙️</span> Editar Cadastro
            </router-link>
        </li>
        <li v-if="userStore.isCliente">
            <router-link to="/funcionario">
                <span class="icon">👥</span> Funcionários
            </router-link>
        </li>

        <li>
          <router-link to="/plano-de-acao">
            <span class="icon">📋</span> Plano de Ação
          </router-link>
        </li>

        <li>
          <router-link to="/relatorio">
            <span class="icon">📊</span> Relatório
          </router-link>
        </li>

        <li>
          <router-link to="/historico">
            <span class="icon">📜</span> Histórico
          </router-link>
        </li>
        
        <li class="logout-item">
          <a @click.prevent="handleLogout" href="#">
            <span class="icon">🚪</span> Sair
          </a>
        </li>
      </ul>
    </nav>

    <div class="main-wrapper">
      <main class="main-content">
        <div class="responder-container">
          
          <div v-if="isLoading" class="loading">
            <span class="loader"></span> Carregando dados...
          </div>
          <div v-else-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </div>

          <div v-else>
            
            <div v-if="userStore.isAdmin || userStore.userRole === 'Psicologo'">
              <header class="page-header">
                <h1 class="content-title">Empresas Clientes</h1>
                </header>
              
              <p class="description">Gerencie as empresas ativas. Clique na empresa para ver detalhes.</p>

              <table v-if="empresas && empresas.length > 0" class="custom-table interactive-table">
                <thead>
                  <tr>
                    <th>Nome da Empresa</th>
                    <th>Responsável</th>
                    <th>Setor</th>
                    <th>CNPJ</th>
                    </tr>
                </thead>
                <tbody>
                  <tr v-for="empresa in empresas" :key="empresa.id" @click="gerirEmpresa(empresa.id)">
                    <td><strong>{{ empresa.nomeEmpresa }}</strong></td>
                    <td>{{ empresa.nomeResponsavel }}</td>
                    <td>{{ empresa.setorAtuacao }}</td>
                    <td>{{ empresa.cnpj }}</td>
                    </tr>
                </tbody>
              </table>
              
              <div v-else class="no-data">
                Nenhuma empresa encontrada.
              </div>
            </div>

            <div v-else-if="userStore.isCliente">
              <header class="page-header">
                <h1 class="content-title">Colaboradores</h1>
                <button class="btn-primary">+ Novo Funcionário</button>
              </header>
              
              <p class="description">Gerencie o cadastro dos colaboradores da sua empresa.</p>

              <table v-if="funcionarios && funcionarios.length > 0" class="custom-table">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Email</th>
                    <th>Cargo</th>
                    <th>Setor</th>
                    <th style="text-align: center">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="func in funcionarios" :key="func.id">
                    <td>{{ func.nome }}</td>
                    <td>{{ func.email }}</td>
                    <td>{{ func.cargo }}</td>
                    <td>{{ func.setor }}</td>
                    <td style="text-align: center">
                      <button class="btn-acao btn-edit">Editar</button>
                      <button class="btn-acao btn-delete">Excluir</button>
                    </td>
                  </tr>
                </tbody>
              </table>
              
              <div v-else class="no-data">
                Nenhum funcionário cadastrado.
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
/* --- FIX DE LAYOUT (CORTE DE TELA) --- */
:global(html), :global(body), :global(#app) {
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden; /* Importante para não rolar a janela toda */
}

/* --- LAYOUT GERAL --- */
.app-layout {
  display: flex;
  height: 100%;
  width: 100%;
  background-color: #f3f4f6;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

/* --- SIDEBAR --- */
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

.sidebar-logo {
  width: 140px;
  display: block;
  margin: 0 auto 2rem auto;
}

.sidebar-nav {
  list-style: none;
  padding: 0;
  margin: 0;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.user-badge {
  padding: 0.75rem;
  margin-bottom: 1.5rem;
  background-color: #f9fafb;
  border-radius: 8px;
  font-weight: 600;
  color: #374151;
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: center;
}

.sidebar-nav li { margin-bottom: 5px; }

.sidebar-nav a {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0.75rem 1rem;
  color: #4b5563;
  text-decoration: none;
  border-radius: 6px;
  margin-bottom: 0.5rem;
  transition: all 0.2s;
  font-size: 0.95rem;
}

.sidebar-nav a:hover, .sidebar-nav a.router-link-active {
  background-color: #eff6ff;
  color: #2563eb;
  font-weight: 600;
}

.logout-item {
  margin-top: auto;
  border-top: 1px solid #f3f4f6;
  padding-top: 1rem;
}
.logout-item a { color: #ef4444; }
.logout-item a:hover { background-color: #fef2f2; color: #dc2626; }

/* --- MAIN WRAPPER --- */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow-y: auto;
}

/* --- CONTEÚDO PRINCIPAL --- */
.main-content {
  flex: 1;
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  background-color: #f3f4f6;
}

.responder-container {
  width: 100%;
  max-width: 1000px;
  background: white;
  padding: 2.5rem;
  border-radius: 12px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

/* --- HEADER DA PÁGINA --- */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  border-bottom: 2px solid #f3f4f6;
  padding-bottom: 1rem;
}

.content-title {
  margin: 0;
  font-size: 1.75rem;
  color: #111827;
}

.description { color: #6b7280; margin-bottom: 1.5rem; }

/* --- TABELAS --- */
.custom-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
}

/* Estilo para tabela interativa (clicável) */
.interactive-table tbody tr {
  cursor: pointer;
  transition: background-color 0.2s, transform 0.1s;
}
.interactive-table tbody tr:hover {
  background-color: #eff6ff; /* Azul claro no hover */
  transform: scale(1.002);
}

.custom-table th {
  background-color: #f9fafb;
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #374151;
  border-bottom: 1px solid #e5e7eb;
}

.custom-table td {
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  color: #4b5563;
}

.custom-table tr:last-child td { border-bottom: none; }

/* --- BOTÕES --- */
.btn-primary {
  background-color: #2563eb;
  color: white;
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
}
.btn-primary:hover { background-color: #1d4ed8; }

.btn-acao {
  padding: 0.4rem 0.8rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 500;
  margin: 0 2px;
  transition: opacity 0.2s;
}
.btn-edit { background-color: #f59e0b; color: white; }
.btn-delete { background-color: #ef4444; color: white; }
.btn-acao:hover { opacity: 0.85; }

/* --- ESTADOS --- */
.loading, .error-message, .no-data {
  text-align: center;
  padding: 3rem;
  color: #6b7280;
}
.error-message { color: #dc2626; }
</style>