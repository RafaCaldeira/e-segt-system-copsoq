<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import { useRouter } from 'vue-router';

// --- Estado ---
const funcionarios = ref<Funcionario[] | null>(null);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- Lógica da Sidebar ---
function handleLogout() {
  userStore.logout();
  router.push('/login');
}

// Computado para mostrar o nome/role na sidebar
const displayName = computed(() => {
  // O seu store (user.ts) tem 'Admin', 'Cliente', 'Psicologo'
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  if (userStore.isCliente && userStore.nomeEmpresa) {
    return userStore.nomeEmpresa; // Ex: "Empresa de Teste SA"
  }
  // Fallback se for cliente mas o nome não carregou
  if (userStore.isCliente) return "Cliente";
  return "Menu";
});

// --- Lógica do Conteúdo Principal (Dashboard) ---
onMounted(async () => {
  // Garantir que estamos logados
  if (!userStore.isLoggedIn) {
    errorMessage.value = "Acesso negado. Por favor, faça o login.";
    isLoading.value = false;
    router.push('/login'); // Redireciona se não estiver logado
    return;
  }
  
  // Apenas 'Cliente' ou 'Admin' podem ver esta página por enquanto
  if (!userStore.isCliente && !userStore.isAdmin) {
     errorMessage.value = "Você não tem permissão para ver esta página.";
     isLoading.value = false;
     return;
  }

  // Se for Cliente, buscar os seus funcionários
  if (userStore.isCliente) {
    isLoading.value = true;
    const data = await apiService.getFuncionarios();
    if (data) {
      funcionarios.value = data;
    } else {
      errorMessage.value = "Não foi possível carregar os dados dos funcionários.";
    }
    isLoading.value = false;
  } else {
    // Se for Admin, não precisa de carregar funcionários por agora
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="app-layout">
    
    <nav class="sidebar">
      <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display">
          <span class="icon"></span> {{ displayName }}
        </li>
        
        <li><a href="#"><span class="icon"></span> Editar Cadastro</a></li>
        <li><a href="#"><span class="icon"></span> Plano de ação</a></li>
        <li><a href="#"><span class="icon"></span> Relatórios</a></li>
        <li><a href="#"><span class="icon"></span> Baixar Roadmap</a></li>
        <li><a href="#"><span class="icon"></span> Histórico</a></li>
        
        <li class="logout-item">
          <a @click="handleLogout" href="#">
            <span class="icon icon-logout"></span> Sair
          </a>
        </li>
      </ul>
    </nav>

    <main class="main-content">

      <div class="responder-container">
        
        <div v-if="isLoading" class="loading">
          A carregar dados...
        </div>

        <div v-else-if="errorMessage" class="error-message">
          {{ errorMessage }}
        </div>

        <div v-else>
          
          <div v-if="userStore.isAdmin">
            <h1 class="content-title">Dashboard do Administrador</h1>
            <p>Bem-vindo! Use esta área para gerir questionários e clientes.</p>
            </div>

          <div v-if="userStore.isCliente">
            <h1 class="content-title">Meus Funcionários</h1>
            <p>Abaixo está a lista de funcionários registados na sua empresa.</p>
            <div v-if="!funcionarios" class="loading">
              A carregar funcionários...
            </div>
            <div v-else-if="funcionarios.length === 0" class="no-data">
              Nenhum funcionário registado.
            </div>
            
            <table v-else class="funcionarios-tabela">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Email</th>
                  <th>Cargo</th>
                  <th>Setor</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="func in funcionarios" :key="func.id">
                  <td>{{ func.nome }}</td>
                  <td>{{ func.email }}</td>
                  <td>{{ func.cargo }}</td>
                  <td>{{ func.setor }}</td>
                  <td>
                    <button class="btn-acao">Editar</button>
                    <button class="btn-acao btn-perigo">Excluir</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          
          <div v-if="userStore.userRole === 'Psicologo'">
             <h1 class="content-title">Dashboard do Psicólogo</h1>
             <p>Bem-vindo! Use esta área para aceder aos relatórios de resultados.</p>
          </div>

        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
/* Copiámos todos os estilos do ResponderQuestionario.vue para manter a consistência */
:global(body) {
  margin: 0;
  background-color: #f0f2f5; 
}

.app-layout {
  display: flex;
  min-height: 100vh;
  font-family: Arial, sans-serif;
}

/* --- 1. Barra Lateral (Sidebar) --- */
.sidebar {
  width: 280px;
  flex-shrink: 0;
  background-color: #ffffff;
  padding: 2rem 1.5rem;
  border-right: 1px solid #e0e0e0;
}
.sidebar-logo {
  width: 150px;
  margin-bottom: 2.5rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}
.sidebar-nav {
  list-style: none;
  padding: 0;
  margin: 0;
}
.sidebar-nav li {
  margin-bottom: 0.5rem;
}

/* Estilo do item de utilizador (NOVO) */
.sidebar-nav li.user-display {
  font-size: 1.2rem;
  font-weight: bold;
  color: #333;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  align-items: center;
}

.sidebar-nav a {
  display: flex; /* <-- CORREÇÃO DE ALINHAMENTO */
  align-items: center; /* <-- CORREÇÃO DE ALINHAMENTO */
  padding: 0.8rem 1rem;
  border-radius: 6px;
  text-decoration: none;
  color: #555;
  font-weight: 500;
  transition: background-color 0.2s, color 0.2s;
}
.sidebar-nav a:hover {
  background-color: #f0f2f5;
}
.sidebar-nav li.active a {
  background-color: #e0eafc; 
  color: #3b82f6; 
  font-weight: bold;
}
.sidebar-nav .icon {
  display: inline-block;
  width: 20px;
  height: 20px;
  margin-right: 0.8rem;
  background-color: #ccc; 
  border-radius: 50%;
  flex-shrink: 0;
}
/* Item de Sair (NOVO) */
.sidebar-nav li.logout-item {
  margin-top: 2rem; /* Espaço antes do Sair */
}
.sidebar-nav li.logout-item a {
  color: #d9534f; /* Vermelho */
  font-weight: bold;
}
.sidebar-nav li.logout-item a:hover {
  background-color: #fdf2f2;
}

/* --- 2. Área de Conteúdo Principal --- */
.main-content {
  flex: 1;
  background-color: #333; /* Fundo escuro */
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  overflow-y: auto;
}

/* O "Card" */
.responder-container { /* Reutilizamos o nome do card */
  max-width: 900px; /* Ajuste se quiser mais largo */
  width: 100%;
  margin: 0;
  padding: 2.5rem 3rem;
  border-radius: 8px;
  background-color: #f4f7f6; /* Fundo cinzento claro do card */
  color: #333; 
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

/* Estados de Carregamento/Erro */
.loading, .error-message, .no-data {
  text-align: center;
  padding: 3rem;
  font-size: 1.2rem;
  color: #555;
}
.error-message { color: #d9534f; }

h1.content-title {
  font-size: 2.2rem;
  color: #333;
  border-bottom: 4px solid #3b82f6; 
  padding-bottom: 0.5rem;
  margin-bottom: 2rem;
  display: inline-block;
}
h2 {
  font-size: 1.5rem;
  margin-bottom: 1rem;
}

/* Estilos da Tabela de Funcionários (Novos) */
.funcionarios-tabela {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
  color: #333;
}
.funcionarios-tabela th,
.funcionarios-tabela td {
  border: 1px solid #ddd;
  padding: 0.8rem 1rem;
  text-align: left;
}
.funcionarios-tabela th {
  background-color: #eee;
}
.btn-acao {
  padding: 0.4rem 0.6rem;
  margin-right: 0.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  background-color: #42b883; 
  color: white;
}
.btn-perigo {
  background-color: #ff6b6b;
}
.btn-acao:hover {
  opacity: 0.8;
}
</style>