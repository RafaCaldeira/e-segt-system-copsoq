<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types';
import { useRouter } from 'vue-router';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

// --- ESTADO ---
const funcionarios = ref<Funcionario[] | null>(null);
const empresas = ref<Empresa[] | null>(null); 
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- AÇÕES ---

// Função para navegar para a lista de funcionários de uma empresa (Admin/Psicólogo)
function gerirEmpresa(empresaId: number) {
  // Redireciona para a página que lista os funcionários, passando o ID da empresa
  // Você pode ajustar o destino conforme sua preferência (ex: '/historico' ou uma página nova '/empresa-detalhe')
  router.push({ 
    path: '/historico', // Ou outra rota que mostre a lista de funcionários dessa empresa
    query: { empresaId: empresaId.toString() } 
  });
}

function irParaNovoFuncionario() {
  router.push('/funcionario'); // Redireciona para a tela de cadastro
}

// --- ON MOUNTED ---
onMounted(async () => {
  if (!userStore.isLoggedIn) {
    errorMessage.value = "Acesso negado. Por favor, faça o login.";
    isLoading.value = false;
    router.push('/login');
    return;
  }
  
  isLoading.value = true;

  try {
    // 1. Lógica para Cliente (Empresa)
    if (userStore.isCliente) {
      const data = await apiService.getFuncionarios();
      funcionarios.value = data || [];
    } 
    // 2. Lógica para Admin
    else if (userStore.isAdmin) {
      const data = await apiService.getEmpresas();
      empresas.value = data || [];
    } 
    // 3. Lógica para Psicólogo (Vê empresas atribuídas)
    else if (userStore.userRole === 'Psicologo') {
      const data = await apiService.getEmpresasParaPsicologo(); 
      empresas.value = data || [];
    }
  } catch (error) {
    console.error(error);
    errorMessage.value = "Erro ao carregar dados do servidor.";
  } finally {
    isLoading.value = false;
  }
});
</script>

<template>
  <div class="app-layout">
    
    <AppSidebar />

    <div class="main-wrapper">
      <main class="main-content">
        <div class="content-wrapper">
          
          <div v-if="isLoading" class="loading">
            <span class="loader"></span> Carregando dados...
          </div>
          
          <div v-else-if="errorMessage" class="error-message">
            {{ errorMessage }}
          </div>

          <div v-else>
            
            <div v-if="userStore.isAdmin || userStore.userRole === 'Psicologo'">
              <header class="page-header">
                <div>
                  <h1 class="content-title">Empresas Clientes</h1>
                  <p class="subtitle">Gerencie as empresas ativas. Clique na empresa para ver detalhes.</p>
                </div>
              </header>
              
              <div class="table-container">
                <table v-if="empresas && empresas.length > 0" class="custom-table interactive-table">
                  <thead>
                    <tr>
                      <th>Nome da Empresa</th>
                      <th>Responsável</th>
                      <th>Setor</th>
                      <th>CNPJ</th>
                      <th style="text-align: right;">Ação</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="empresa in empresas" :key="empresa.id" @click="gerirEmpresa(empresa.id)">
                      <td><strong>{{ empresa.nomeEmpresa }}</strong></td>
                      <td>{{ empresa.nomeResponsavel }}</td>
                      <td>{{ empresa.setorAtuacao }}</td>
                      <td>{{ empresa.cnpj }}</td>
                      <td style="text-align: right;">
                        <span class="btn-link">Ver Funcionários →</span>
                      </td>
                    </tr>
                  </tbody>
                </table>
                
                <div v-else class="no-data">
                  <div class="empty-icon">🏢</div>
                  <p>Nenhuma empresa encontrada.</p>
                </div>
              </div>
            </div>

            <div v-else-if="userStore.isCliente">
              <header class="page-header">
                <div>
                  <h1 class="content-title">Meus Colaboradores</h1>
                  <p class="subtitle">Gerencie o cadastro da sua equipe.</p>
                </div>
                <button class="btn-primary" @click="irParaNovoFuncionario">+ Novo Funcionário</button>
              </header>
              
              <div class="table-container">
                <table v-if="funcionarios && funcionarios.length > 0" class="custom-table">
                  <thead>
                    <tr>
                      <th>Nome</th>
                      <th>Email</th>
                      <th>Cargo</th>
                      <th>Setor</th>
                      </tr>
                  </thead>
                  <tbody>
                    <tr v-for="func in funcionarios" :key="func.id">
                      <td><strong>{{ func.nome }}</strong></td>
                      <td>{{ func.email }}</td>
                      <td>{{ func.cargo || '-' }}</td>
                      <td>{{ func.setor }}</td>
                    </tr>
                  </tbody>
                </table>
                
                <div v-else class="no-data">
                  <div class="empty-icon">👥</div>
                  <p>Você ainda não tem funcionários cadastrados.</p>
                  <button class="btn-secondary" @click="irParaNovoFuncionario">Cadastrar Primeiro Funcionário</button>
                </div>
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
/* --- Layout Global --- */
:global(html), :global(body), :global(#app) {
  height: 100%; margin: 0; padding: 0; overflow: hidden;
}
:global(body) { background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif; }

.app-layout { display: flex; height: 100%; width: 100%; }

/* --- Main Wrapper --- */
.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow-y: auto;
}

/* --- Main Content --- */
.main-content {
  flex: 1; background-color: #f0f2f5; padding: 2rem;
  display: flex; justify-content: center; align-items: flex-start;
}

.content-wrapper {
  max-width: 1100px; width: 100%; background: white; padding: 2.5rem;
  border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 2rem;
}

/* --- Header --- */
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; border-bottom: 2px solid #f3f4f6; padding-bottom: 1rem; flex-wrap: wrap; gap: 1rem; }
.content-title { font-size: 1.8rem; color: #111; margin: 0; }
.subtitle { color: #6b7280; margin: 5px 0 0 0; }

/* --- Tabelas --- */
.table-container { overflow-x: auto; }
.custom-table {
  width: 100%; border-collapse: separate; border-spacing: 0;
  border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden;
}

/* Interatividade para linhas clicáveis */
.interactive-table tbody tr {
  cursor: pointer; transition: background-color 0.2s, transform 0.1s;
}
.interactive-table tbody tr:hover {
  background-color: #eff6ff;
}

.custom-table th {
  background-color: #f9fafb; padding: 1rem; text-align: left;
  font-weight: 600; color: #374151; border-bottom: 1px solid #e5e7eb;
}
.custom-table td {
  padding: 1rem; border-bottom: 1px solid #e5e7eb; color: #4b5563; vertical-align: middle;
}
.custom-table tr:last-child td { border-bottom: none; }

.btn-link { color: #2563eb; font-weight: 600; font-size: 0.9rem; }

/* --- Botões --- */
.btn-primary { background: #2563eb; color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; font-size: 0.95rem; }
.btn-primary:hover { background: #1d4ed8; }

.btn-secondary { background: #f3f4f6; color: #374151; border: 1px solid #e5e7eb; padding: 0.6rem 1.2rem; border-radius: 6px; font-weight: 500; cursor: pointer; margin-top: 1rem; }
.btn-secondary:hover { background: #e5e7eb; }

/* --- Estados (Loading/Empty) --- */
.loading, .error-message { text-align: center; padding: 3rem; color: #6b7280; font-size: 1.1rem; }
.error-message { color: #dc2626; }

.no-data { text-align: center; padding: 4rem 2rem; background: #f9fafb; border-radius: 8px; border: 2px dashed #e5e7eb; }
.empty-icon { font-size: 3rem; margin-bottom: 1rem; opacity: 0.5; }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
  .page-header { flex-direction: column; align-items: flex-start; }
  .btn-primary { width: 100%; margin-top: 1rem; }
}
</style>