<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
import type { Funcionario } from '../types/funcionario.types';
import type { Empresa } from '../types/empresa.types';
import { useRouter } from 'vue-router';
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

// --- ESTADO ---
const funcionarios = ref<Funcionario[] | null>(null);
const empresas = ref<Empresa[] | null>(null); 
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

const userStore = useUserStore();
const router = useRouter();

// --- HELPER: Gerar Iniciais para Avatar ---
function getInitials(name: string) {
  if (!name) return '??';
  
  // O .filter(Boolean) remove espaços vazios extras caso o nome tenha dois espaços
  const names = name.split(' ').filter(Boolean);
  
  if (names.length === 0) return '??';

  const first = names[0];
  const last = names[names.length - 1];

  // TypeScript agora sabe que 'first' existe
  if (names.length === 1 && first) {
    return first.substring(0, 2).toUpperCase();
  }

  // Garante que 'first' e 'last' existem antes de acessar a primeira letra
  if (first && last) {
    return (first.charAt(0) + last.charAt(0)).toUpperCase();
  }

  return '??';
}

// --- HELPER: Cor de fundo baseada no nome (consistente) ---
function getAvatarColor(name: string) {
  const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return colors[Math.abs(hash) % colors.length];
}

// --- AÇÕES ---
function gerirEmpresa(empresaId: number) {
  router.push({ 
    path: '/historico', 
    query: { empresaId: empresaId.toString() } 
  });
}

function irParaNovoFuncionario() {
  router.push('/funcionario');
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
    if (userStore.isCliente) {
      const data = await apiService.getFuncionarios();
      funcionarios.value = data || [];
    } else if (userStore.isAdmin) {
      const data = await apiService.getEmpresas();
      empresas.value = data || [];
    } else if (userStore.userRole === 'Psicologo') {
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
        <header class="main-header" v-if="!isLoading && !errorMessage">
          <div v-if="userStore.isAdmin || userStore.userRole === 'Psicologo'">
             <h1 class="page-title">Painel de Empresas</h1>
             <p class="page-subtitle">Visão geral das empresas parceiras ativas no sistema.</p>
          </div>
          <div v-else-if="userStore.isCliente" class="header-flex">
             <div>
                <h1 class="page-title">Minha Equipe</h1>
                <p class="page-subtitle">Gerencie os colaboradores da sua empresa.</p>
             </div>
             <button class="btn-primary glow-effect" @click="irParaNovoFuncionario">
                <span>+</span> Novo Funcionário
             </button>
          </div>
        </header>

        <div class="content-wrapper">
          
          <div v-if="isLoading" class="state-container">
            <div class="spinner"></div>
            <p>Sincronizando dados...</p>
          </div>
          
          <div v-else-if="errorMessage" class="state-container error">
            <span class="icon">⚠️</span>
            <p>{{ errorMessage }}</p>
          </div>

          <div v-else class="fade-in">
            
            <div v-if="userStore.isAdmin || userStore.userRole === 'Psicologo'">
              <div class="table-responsive">
                <table v-if="empresas && empresas.length > 0" class="modern-table">
                  <thead>
                    <tr>
                      <th>Empresa</th>
                      <th>Responsável</th>
                      <th>Setor</th>
                      <th>CNPJ</th>
                      <th class="text-right">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="empresa in empresas" :key="empresa.id" @click="gerirEmpresa(empresa.id)">
                      <td>
                        <div class="company-info">
                          <div class="avatar" :style="{ backgroundColor: getAvatarColor(empresa.nomeEmpresa) }">
                            {{ getInitials(empresa.nomeEmpresa) }}
                          </div>
                          <span class="company-name">{{ empresa.nomeEmpresa }}</span>
                        </div>
                      </td>
                      <td class="text-secondary">{{ empresa.nomeResponsavel }}</td>
                      <td>
                        <span class="badge">{{ empresa.setorAtuacao }}</span>
                      </td>
                      <td class="font-mono">{{ empresa.cnpj }}</td>
                      <td class="text-right">
                        <button class="btn-ghost">
                          Gerenciar <span class="arrow">→</span>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
                
                <div v-else class="empty-state">
                  <div class="empty-illustration">🏢</div>
                  <h3>Nenhuma empresa cadastrada</h3>
                  <p>As empresas aparecerão aqui assim que forem registradas.</p>
                </div>
              </div>
            </div>

            <div v-else-if="userStore.isCliente">
              <div class="table-responsive">
                <table v-if="funcionarios && funcionarios.length > 0" class="modern-table">
                  <thead>
                    <tr>
                      <th>Colaborador</th>
                      <th>Contato</th>
                      <th>Cargo</th>
                      <th>Setor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="func in funcionarios" :key="func.id">
                      <td>
                        <div class="company-info">
                          <div class="avatar circle" :style="{ backgroundColor: getAvatarColor(func.nome) }">
                            {{ getInitials(func.nome) }}
                          </div>
                          <span class="company-name">{{ func.nome }}</span>
                        </div>
                      </td>
                      <td class="text-secondary">{{ func.email }}</td>
                      <td>{{ func.cargo || '-' }}</td>
                      <td><span class="badge blue">{{ func.setor }}</span></td>
                    </tr>
                  </tbody>
                </table>
                
                <div v-else class="empty-state">
                  <div class="empty-illustration">👥</div>
                  <h3>Sua equipe está vazia</h3>
                  <p>Comece adicionando os colaboradores para gerenciar a segurança.</p>
                  <button class="btn-secondary" @click="irParaNovoFuncionario">Cadastrar Primeiro</button>
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
/* --- 1. CONFIGURAÇÕES GERAIS --- */
:global(body) { 
  background-color: #f3f4f6; 
  font-family: 'Inter', 'Segoe UI', sans-serif; /* Fonte mais moderna se disponível */
  color: #1f2937;
}

.app-layout { 
  display: flex; 
  height: 100vh; 
  width: 100%; 
  overflow: hidden; 
}

.main-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow-y: auto;
}

.main-content {
  flex: 1; 
  background-color: #f3f4f6; 
  padding: 2rem 3rem; /* Mais espaçamento lateral */
  display: flex; 
  flex-direction: column; 
}

/* --- 2. HEADER DA PÁGINA --- */
.main-header {
  margin-bottom: 2rem;
}
.header-flex {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.page-title {
  font-size: 1.85rem;
  font-weight: 700;
  color: #111827;
  margin: 0 0 0.5rem 0;
  letter-spacing: -0.025em;
}
.page-subtitle {
  color: #6b7280;
  margin: 0;
  font-size: 1rem;
}

/* --- 3. WRAPPER BRANCO (CARD PRINCIPAL) --- */
.content-wrapper {
  width: 100%;
  background: white;
  border-radius: 16px; /* Bordas mais arredondadas */
  /* Sombra suave e moderna */
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05), 0 4px 6px -2px rgba(0, 0, 0, 0.025);
  display: flex;
  flex-direction: column;
  overflow: hidden; /* Para o border-radius funcionar na tabela */
  margin-bottom: 3rem;
}

/* --- 4. TABELA MODERNA --- */
.table-responsive {
  width: 100%;
  overflow-x: auto;
}

.modern-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.modern-table th {
  background-color: #f9fafb;
  color: #6b7280;
  font-weight: 600;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modern-table td {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #f3f4f6;
  color: #374151;
  font-size: 0.95rem;
  vertical-align: middle;
}

/* Efeito de Hover na linha */
.modern-table tbody tr {
  cursor: pointer;
  transition: all 0.2s ease;
}
.modern-table tbody tr:hover {
  background-color: #f8fafc;
  transform: translateY(-1px); /* Leve efeito de elevação */
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}

.modern-table tr:last-child td {
  border-bottom: none;
}

/* --- 5. ELEMENTOS VISUAIS (AVATAR, BADGE, BUTTONS) --- */

/* Avatar */
.company-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 8px; /* Quadrado arredondado para empresas */
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.9rem;
  flex-shrink: 0;
}
.avatar.circle {
  border-radius: 50%; /* Redondo para pessoas */
}
.company-name {
  font-weight: 600;
  color: #111827;
}

/* Badges */
.badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: #e0f2fe;
  color: #0369a1;
}
.badge.blue { background-color: #e0e7ff; color: #4338ca; }

/* Font Mono para números */
.font-mono {
  font-family: 'Courier New', Courier, monospace;
  color: #6b7280;
}

/* Botões */
.btn-primary {
  background-color: #2563eb;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.btn-primary:hover {
  background-color: #1d4ed8;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(37, 99, 235, 0.2);
}

.btn-ghost {
  background: transparent;
  border: 1px solid transparent;
  color: #2563eb;
  font-weight: 600;
  font-size: 0.85rem;
  cursor: pointer;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: background 0.2s;
}
.btn-ghost:hover {
  background-color: #eff6ff;
}
.arrow { transition: transform 0.2s; display: inline-block; }
.btn-ghost:hover .arrow { transform: translateX(3px); }

.btn-secondary {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
}
.btn-secondary:hover { background: #f9fafb; border-color: #9ca3af; }

/* Utilitários */
.text-right { text-align: right; }
.text-secondary { color: #6b7280; }

/* --- 6. ESTADOS (EMPTY / LOADING) --- */
.state-container {
  padding: 4rem;
  text-align: center;
  color: #6b7280;
}
.spinner {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #2563eb;
  border-radius: 50%;
  width: 30px;
  height: 30px;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
}
.empty-illustration { font-size: 3rem; margin-bottom: 1rem; opacity: 0.3; }
.empty-state h3 { margin: 0 0 0.5rem; color: #374151; }
.empty-state p { margin: 0; color: #9ca3af; }

/* Animação de entrada */
.fade-in {
  animation: fadeIn 0.4s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Mobile */
@media (max-width: 768px) {
  .main-content { padding: 1rem; }
  .header-flex { flex-direction: column; align-items: flex-start; gap: 1rem; }
  .btn-primary { width: 100%; justify-content: center; }
  .modern-table th, .modern-table td { padding: 1rem; }
}
</style>