<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user';
import { apiService } from '../services/api.service';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppFooter from '../components/AppFooter.vue';
import AppSidebar from '../components/AppSidebar.vue';

const userStore = useUserStore();
const router = useRouter();

// --- ESTADO ---
const isLoading = ref(true);
const isSaving = ref(false);
const message = ref<{ text: string; type: 'success' | 'error' } | null>(null);

// Modelo do Formulário
const form = ref({
  nome: '',
  email: '',
  nomeEmpresa: '', // Apenas para Cliente
  senhaAtual: '',
  novaSenha: '',
  confirmarSenha: ''
});

// --- CARREGAR DADOS ---
onMounted(async () => {
  if (!userStore.isLoggedIn) { router.push('/login'); return; }

  // Verificação de segurança: Se o ID não estiver na store, força logout ou avisa
  if (!userStore.userId) {
    message.value = { text: "ID do usuário não encontrado. Faça login novamente.", type: 'error' };
    isLoading.value = false;
    return;
  }

  try {
    // Busca os dados usando o ID garantido
    const dadosUsuario = await apiService.getUsuarioAtual(userStore.userId); 
    
    if (dadosUsuario) {
      form.value.nome = dadosUsuario.nome;
      form.value.email = dadosUsuario.email;
      if (userStore.isCliente) {
        form.value.nomeEmpresa = dadosUsuario.nomeEmpresa || '';
      }
    }
  } catch (error) {
    console.error(error);
    message.value = { text: "Erro ao carregar seus dados.", type: 'error' };
  } finally {
    isLoading.value = false;
  }
});

// --- SALVAR ---
async function salvarAlteracoes() {
  message.value = null;

  // 1. Validação básica
  if (!form.value.nome || !form.value.email) {
    message.value = { text: "Nome e Email são obrigatórios.", type: 'error' };
    return;
  }
  
  // 2. Verificação de ID
  if (!userStore.userId) {
    message.value = { text: "Sessão inválida. Faça login novamente.", type: 'error' };
    return;
  }
  const idUsuario = userStore.userId; // Variável local para garantir tipagem

  // 3. Validação de senha
  if (form.value.novaSenha) {
    if (form.value.novaSenha !== form.value.confirmarSenha) {
      message.value = { text: "A nova senha e a confirmação não conferem.", type: 'error' };
      return;
    }
    if (form.value.novaSenha.length < 6) {
        message.value = { text: "A nova senha deve ter no mínimo 6 caracteres.", type: 'error' };
        return;
    }
  }

  isSaving.value = true;

  try {
    const payload: any = {
      id: idUsuario,
      nome: form.value.nome,
      email: form.value.email,
    };

    if (form.value.novaSenha) {
      payload.senha = form.value.novaSenha;
    }

    if (userStore.isCliente) {
      payload.nomeEmpresa = form.value.nomeEmpresa;
    }

    const sucesso = await apiService.updateUsuario(idUsuario, payload);

    if (sucesso) {
      message.value = { text: "Dados atualizados com sucesso!", type: 'success' };
      
      // Atualiza a store localmente para refletir mudanças imediatas na UI
      if (userStore.isCliente) userStore.nomeEmpresa = form.value.nomeEmpresa;
      
      form.value.novaSenha = '';
      form.value.confirmarSenha = '';
    } else {
      message.value = { text: "Erro ao atualizar. Tente novamente.", type: 'error' };
    }
  } catch (error) {
    message.value = { text: "Erro de conexão com o servidor.", type: 'error' };
  } finally {
    isSaving.value = false;
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
            <h1 class="content-title">Editar Meus Dados</h1>
            <p class="subtitle">Mantenha suas informações de acesso atualizadas.</p>
          </header>

          <div v-if="message" :class="['alert', message.type]">
            {{ message.text }}
          </div>

          <div v-if="isLoading" class="loading-state">
            <div class="spinner"></div> Carregando informações...
          </div>

          <form v-else @submit.prevent="salvarAlteracoes" class="edit-form">
            
            <div class="form-section">
              <h3>Informações Pessoais</h3>
              <div class="form-group">
                <label>Nome Completo / Responsável</label>
                <input v-model="form.nome" type="text" required />
              </div>

              <div class="form-group">
                <label>Email de Acesso</label>
                <input v-model="form.email" type="email" required />
              </div>

              <div v-if="userStore.isCliente" class="form-group">
                <label>Nome da Empresa</label>
                <input v-model="form.nomeEmpresa" type="text" required />
              </div>
            </div>

            <div class="form-section">
              <h3>Segurança</h3>
              <p class="hint">Preencha apenas se quiser alterar sua senha.</p>
              
              <div class="pass-grid">
                <div class="form-group">
                  <label>Nova Senha</label>
                  <input v-model="form.novaSenha" type="password" placeholder="Mínimo 6 caracteres" />
                </div>

                <div class="form-group">
                  <label>Confirmar Nova Senha</label>
                  <input v-model="form.confirmarSenha" type="password" placeholder="Repita a senha" />
                </div>
              </div>
            </div>

            <div class="action-footer">
              <button type="submit" class="btn-save" :disabled="isSaving">
                {{ isSaving ? 'Salvando...' : '💾 Salvar Alterações' }}
              </button>
            </div>

          </form>

        </div>
      </main>

      <AppFooter />
    </div>

  </div>
</template>

<style scoped>
/* Layout Fix */
:global(html), :global(body), :global(#app) { height: 100%; margin: 0; padding: 0; overflow: hidden; }
:global(body) { background-color: #f0f2f5; font-family: 'Segoe UI', sans-serif; }

.app-layout { display: flex; height: 100%; width: 100%; }

/* Main Wrapper */
.main-wrapper { 
  flex: 1; 
  display: flex; 
  flex-direction: column; 
  height: 100vh; 
  overflow-y: auto; 
}

/* Main Content */
.main-content { 
  flex: 1; 
  padding: 2rem; 
  display: flex; 
  justify-content: center; 
  align-items: flex-start; 
  background-color: #f0f2f5; 
}

.content-wrapper { 
  max-width: 800px; 
  width: 100%; 
  background: white; 
  padding: 2.5rem; 
  border-radius: 12px; 
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); 
  margin-bottom: 2rem; 
}

/* Styles Específicos */
.page-header { margin-bottom: 2rem; border-bottom: 1px solid #eee; padding-bottom: 1rem; }
.content-title { font-size: 1.8rem; color: #1f2937; margin: 0; }
.subtitle { color: #6b7280; margin: 5px 0 0 0; }

.form-section { margin-bottom: 2rem; }
.form-section h3 { color: #374151; margin-bottom: 1rem; border-left: 4px solid #3b82f6; padding-left: 10px; }
.hint { font-size: 0.9rem; color: #6b7280; margin-bottom: 1rem; font-style: italic; }

.form-group { margin-bottom: 1rem; }
.form-group label { display: block; font-weight: 600; margin-bottom: 0.5rem; color: #4b5563; font-size: 0.95rem; }
.form-group input { width: 100%; padding: 0.8rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: 1rem; box-sizing: border-box; transition: border 0.2s; }
.form-group input:focus { border-color: #3b82f6; outline: none; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }

.pass-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }

.action-footer { margin-top: 2rem; padding-top: 1.5rem; border-top: 1px solid #f3f4f6; text-align: right; }
.btn-save { background-color: #2563eb; color: white; padding: 0.8rem 2rem; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; font-size: 1rem; transition: background 0.2s; }
.btn-save:hover:not(:disabled) { background-color: #1d4ed8; }
.btn-save:disabled { background-color: #93c5fd; cursor: not-allowed; }

/* Alerts & Loading */
.alert { padding: 1rem; border-radius: 6px; margin-bottom: 1.5rem; text-align: center; font-weight: 500; }
.alert.success { background: #d1fae5; color: #065f46; border: 1px solid #a7f3d0; }
.alert.error { background: #fee2e2; color: #991b1b; border: 1px solid #fecaca; }

.loading-state { text-align: center; padding: 3rem; color: #6b7280; }
.spinner { display: inline-block; width: 24px; height: 24px; border: 3px solid #e2e8f0; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-right: 10px; vertical-align: middle; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
  .pass-grid { grid-template-columns: 1fr; }
}
</style>