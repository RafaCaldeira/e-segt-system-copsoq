<script setup lang="ts">
import { computed } from 'vue';
import { useUserStore } from '../store/user';
import { useRouter } from 'vue-router';

const userStore = useUserStore();
const router = useRouter();

// Exibe o Cargo ou o Nome da Empresa
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole || 'Usuário');

function handleLogout() {
  userStore.logout();
  router.push('/login');
}
</script>

<template>
  <nav class="sidebar">
    <div class="logo-area">
      <router-link to="/dashboard">
        <img src="../assets/e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      </router-link>
    </div>
    
    <div class="user-badge">{{ displayName }}</div>

    <ul class="sidebar-nav">
      
      <li v-if="userStore.isAdmin">
        <router-link to="/criar-questionario" active-class="active-link">
          <span class="icon">📝</span> Criar Questionário
        </router-link>
      </li>
      <li v-if="userStore.isAdmin">
        <router-link to="/disparo" active-class="active-link">
          <span class="icon">📨</span> Enviar Questionário
        </router-link>
      </li>

      <li v-if="userStore.isCliente">
        <router-link to="/editar-cadastro" active-class="active-link">
          <span class="icon">⚙️</span> Editar Cadastro
        </router-link>
      </li>
      <li v-if="userStore.isCliente">
        <router-link to="/funcionario" active-class="active-link">
          <span class="icon">👥</span> Funcionários
        </router-link>
      </li>

      <li v-if="userStore.userRole === 'Psicologo'">
        <router-link to="/psicologo" active-class="active-link">
          <span class="icon">🧠</span> Área do Psicólogo
        </router-link>
      </li>

      <li>
        <router-link to="/plano-de-acao" active-class="active-link">
          <span class="icon">📋</span> Plano de Ação
        </router-link>
      </li>
      <li>
        <router-link to="/relatorio" active-class="active-link">
          <span class="icon">📊</span> Relatórios
        </router-link>
      </li>
      <li>
        <router-link to="/historico" active-class="active-link">
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
</template>

<style scoped>
/* CSS Padrão da Sidebar */
.sidebar { 
  width: 260px; 
  background-color: #ffffff; 
  border-right: 1px solid #e5e7eb; 
  display: flex; 
  flex-direction: column; 
  padding: 1.5rem 1rem; 
  flex-shrink: 0; 
  z-index: 10;
  height: 100vh;
  position: sticky;
  top: 0;
}

/* Cursor pointer para indicar clique */
.sidebar-logo { 
  width: 120px; 
  display: block; 
  margin: 0 auto 1.5rem auto; 
  cursor: pointer; 
  transition: transform 0.2s;
}

.sidebar-logo:hover {
  transform: scale(1.05);
}

.user-badge { 
  background: #f3f4f6; 
  padding: 0.5rem; 
  border-radius: 6px; 
  text-align: center; 
  font-weight: bold; 
  margin-bottom: 1.5rem; 
  color: #374151; 
  font-size: 0.9rem;
}

.sidebar-nav { list-style: none; padding: 0; margin: 0; flex: 1; overflow-y: auto; }
.sidebar-nav li { margin-bottom: 5px; }

.sidebar-nav a { 
  display: flex; 
  align-items: center; 
  padding: 0.75rem 1rem; 
  color: #4b5563; 
  text-decoration: none; 
  border-radius: 6px; 
  font-weight: 500; 
  transition: all 0.2s; 
}

.sidebar-nav a:hover { background: #f3f4f6; color: #111; }

.active-link { background: #eff6ff !important; color: #2563eb !important; font-weight: 600; }

.sidebar-nav .icon { margin-right: 10px; min-width: 20px; text-align: center; }

.logout-item { 
  margin-top: auto; 
  border-top: 1px solid #f3f4f6; 
  padding-top: 1rem; 
}
.logout-item a { color: #ef4444; }
.logout-item a:hover { background: #fef2f2; }

/* Responsivo */
@media (max-width: 768px) {
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; position: relative; }
  .sidebar-nav { display: flex; flex-wrap: wrap; gap: 10px; justify-content: center; }
  .sidebar-nav li { margin: 0; }
  .logout-item { margin-top: 0; width: 100%; text-align: center; }
  .logout-item a { justify-content: center; }
}
</style>