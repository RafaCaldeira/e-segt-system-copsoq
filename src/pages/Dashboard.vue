<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types'; // <-- Importar Empresa
import { useRouter } from 'vue-router';

// Estado
const funcionarios = ref<Funcionario[] | null>(null);
const empresas = ref<Empresa[] | null>(null); // <-- Novo estado para Empresas
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- Lógica da Sidebar ---
function handleLogout() {
  userStore.logout();
  router.push('/login');
}

const displayName = computed(() => {
  if (userStore.userRole === 'Admin') return "Administrador";
  if (userStore.userRole === 'Psicologo') return "Psicólogo";
  if (userStore.isCliente && userStore.nomeEmpresa) {
    return userStore.nomeEmpresa;
  }
  if (userStore.isCliente) return "Cliente"; 
  return "Menu";
});

// --- Lógica do Conteúdo Principal (Dashboard) ---
onMounted(async () => {
  if (!userStore.isLoggedIn) {
    errorMessage.value = "Acesso negado. Por favor, faça o login.";
    isLoading.value = false;
    router.push('/login');
    return;
  }
  
  isLoading.value = true;

  // --- LÓGICA ATUALIZADA ---
  if (userStore.isCliente) {
    // 1. Se for Cliente, buscar os seus funcionários
    const data = await apiService.getFuncionarios();
    if (data) {
      funcionarios.value = data;
    } else {
      errorMessage.value = "Não foi possível carregar os dados dos funcionários.";
    }
  } 
  else if (userStore.isAdmin) {
    // 2. Se for Admin, buscar a lista de empresas
    const data = await apiService.getEmpresas();
    if (data) {
      empresas.value = data;
    } else {
      errorMessage.value = "Não foi possível carregar a lista de empresas.";
    }
  }
  // (Psicologo não carrega nada por enquanto)
  
  isLoading.value = false;
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
            <h1 class="content-title">Empresas Clientes</h1>
            <p>Abaixo está a lista de todas as empresas clientes ativas.</p>

            <div v-if="!empresas" class="loading">
              A carregar empresas...
            </div>
            <div v-else-if="empresas.length === 0" class="no-data">
              Nenhuma empresa cliente registada.
            </div>
            
            <table v-else class="funcionarios-tabela"> <thead>
                <tr>
                  <th>Nome da Empresa</th>
                  <th>Responsável</th>
                  <th>Setor</th>
                  <th>CNPJ</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="empresa in empresas" :key="empresa.id">
                  <td>{{ empresa.nomeEmpresa }}</td>
                  <td>{{ empresa.nomeResponsavel }}</td>
                  <td>{{ empresa.setorAtuacao }}</td>
                  <td>{{ empresa.cnpj }}</td>
                  <td>
                    <button class="btn-acao">Gerir</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-else-if="userStore.isCliente">
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
          
          <div v-else-if="userStore.userRole === 'Psicologo'">
             <h1 class="content-title">Dashboard do Psicólogo</h1>
             <p>Bem-vindo! Use esta área para aceder aos relatórios de resultados.</p>
          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* (O seu CSS existente está ótimo, não precisa de alterações,
   pois estamos a reutilizar as classes) */

:global(body) {
  margin: 0;
  background-color: #f0f2f5; 
}
.app-layout {
  display: flex;
  min-height: 100vh;
  font-family: Arial, sans-serif;
}
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
  display: flex;
  align-items: center;
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
.sidebar-nav li.logout-item {
  margin-top: 2rem;
}
.sidebar-nav li.logout-item a {
  color: #d9534f;
  font-weight: bold;
}
.sidebar-nav li.logout-item a:hover {
  background-color: #fdf2f2;
}
.main-content {
  flex: 1;
  background-color: #333;
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  overflow-y: auto;
}
.responder-container {
  max-width: 900px;
  width: 100%;
  margin: 0;
  padding: 2.5rem 3rem;
  border-radius: 8px;
  background-color: #f4f7f6;
  color: #333; 
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}
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