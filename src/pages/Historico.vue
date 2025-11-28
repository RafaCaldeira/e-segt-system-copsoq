<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useUserStore } from '../store/user';
import { apiService, type DisparoHistoricoDto } from '../services/api.service';
import { useRouter } from 'vue-router';

const userStore = useUserStore();
const router = useRouter();

const historico = ref<DisparoHistoricoDto[]>([]);
const isLoading = ref(true);

// Sidebar
function handleLogout() { userStore.logout(); router.push('/login'); }
const displayName = computed(() => userStore.nomeEmpresa || userStore.userRole);

onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }
  await carregarHistorico();
});

async function carregarHistorico() {
  isLoading.value = true;
  const data = await apiService.getHistoricoDisparos();
  if (data) {
    historico.value = data;
  }
  isLoading.value = false;
}

function copiarLink(token: string) {
  const urlCompleta = `${window.location.origin}/responder/${token}`;
  navigator.clipboard.writeText(urlCompleta).then(() => {
    alert("Link copiado para a área de transferência!");
  });
}

function formatarData(dataIso: string) {
  if (!dataIso) return '-';
  return new Date(dataIso).toLocaleDateString('pt-BR') + ' ' + new Date(dataIso).toLocaleTimeString('pt-BR', { hour: '2-digit', minute:'2-digit' });
}
</script>

<template>
  <div class="app-layout">
    <nav class="sidebar">
      <img src="../assets/logo-e-segt.png" alt="E-SegT Logo" class="sidebar-logo">
      <ul class="sidebar-nav">
        <li class="user-display"><span class="icon"></span> {{ displayName }}</li>
        <li><router-link to="/dashboard"><span class="icon"></span> Dashboard</router-link></li>
        <!-- Mostra links diferentes dependendo da Role -->
        <li v-if="userStore.isAdmin"><router-link to="/disparar"><span class="icon"></span> Disparar Formulários</router-link></li>
        <li class="active"><a href="#"><span class="icon"></span> Histórico</a></li>
        <li class="logout-item"><a @click="handleLogout" href="#"><span class="icon icon-logout"></span> Sair</a></li>
      </ul>
    </nav>

    <main class="main-content">
      <div class="responder-container">
        <h1 class="content-title">Histórico de Envios</h1>
        <p class="desc">Acompanhe o status dos questionários enviados e copie os links.</p>

        <div v-if="isLoading" class="loading">Carregando...</div>
        
        <div v-else>
           <table class="history-table">
            <thead>
              <tr>
                <th>Funcionário</th>
                <th>Questionário</th>
                <th>Data Envio</th>
                <th>Status</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in historico" :key="item.id">
                <td>
                  <strong>{{ item.nomeFuncionario }}</strong><br>
                  <span class="email-small">{{ item.emailFuncionario }}</span>
                </td>
                <td>{{ item.tituloQuestionario }}</td>
                <td>{{ formatarData(item.dataEnvio) }}</td>
                <td>
                  <span class="status-badge" :class="item.respondido ? 'respondido' : 'pendente'">
                    {{ item.respondido ? 'Respondido' : 'Pendente' }}
                  </span>
                </td>
                <td>
                  <button v-if="!item.respondido" class="btn-copy" @click="copiarLink(item.link)">
                    🔗 Copiar Link
                  </button>
                </td>
              </tr>
            </tbody>
           </table>
           
           <div v-if="historico.length === 0" class="no-data">
             Nenhum envio registado.
           </div>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* Reutilizando estilos globais */
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
.content-title { font-size: 2rem; color: #333; border-bottom: 4px solid #3b82f6; padding-bottom: 0.5rem; margin-bottom: 1.5rem; display: inline-block; }
.desc { color: #666; margin-bottom: 2rem; }
.loading { text-align: center; padding: 3rem; }
.no-data { text-align: center; padding: 3rem; color: #888; font-style: italic; }

/* Tabela de Histórico */
.history-table { width: 100%; border-collapse: collapse; background-color: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.history-table th, .history-table td { padding: 1rem; text-align: left; border-bottom: 1px solid #eee; }
.history-table th { background-color: #f8f9fa; font-weight: bold; color: #555; text-transform: uppercase; font-size: 0.85rem; }
.email-small { font-size: 0.85rem; color: #777; }

.status-badge { padding: 0.3rem 0.8rem; border-radius: 20px; font-size: 0.85rem; font-weight: bold; color: white; }
.status-badge.pendente { background-color: #f0ad4e; }
.status-badge.respondido { background-color: #28a745; }

.btn-copy { padding: 0.4rem 0.8rem; background-color: #17a2b8; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 0.9rem; transition: opacity 0.2s; }
.btn-copy:hover { opacity: 0.8; }
</style>