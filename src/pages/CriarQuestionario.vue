<script setup lang="ts">
import { ref, computed } from 'vue';
import { apiService } from '../services/api.service';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user';
// 1. IMPORTAR COMPONENTES PADRÃO
import AppSidebar from '../components/AppSidebar.vue';
import AppFooter from '../components/AppFooter.vue';

// --- TIPOS ---
interface QuestionarioCreateDto {
  titulo: string;
  descricao: string;
  textoIntroducao: string;
  textoConsentimento: string;
  setoresAplicaveis: string[];
}

interface OpcaoRespostaCreateDto {
  texto: string;
  valor: number;
  ordem: number;
}

interface PerguntaLocal {
  texto: string;
}

interface DimensaoLocal {
  tempId: number;
  titulo: string;
  nomeIndicador: string;
  ordem: number;
  perguntas: PerguntaLocal[];
}

// --- CONFIG ---
const router = useRouter();
const userStore = useUserStore();

// --- ESTADO ---
const step = ref(1); 
const isLoading = ref(false);
const novoQuestionarioId = ref<number | null>(null);

// Passo 1: Info Básica
const questionarioData = ref<QuestionarioCreateDto>({
  titulo: '',
  descricao: '',
  textoIntroducao: '',
  textoConsentimento: '',
  setoresAplicaveis: []
});

// Passo 2: Escala
const escalaOpcoes = ref<OpcaoRespostaCreateDto[]>([
  { texto: 'Nunca', valor: 1, ordem: 1 },
  { texto: 'Raramente', valor: 2, ordem: 2 },
  { texto: 'Às vezes', valor: 3, ordem: 3 },
  { texto: 'Frequentemente', valor: 4, ordem: 4 },
  { texto: 'Sempre', valor: 5, ordem: 5 }
]);

// Passo 3: Dimensões
const dimensoes = ref<DimensaoLocal[]>([]);

// --- AÇÕES ---

// Navegação entre passos
function prevStep() {
  if (step.value > 1) step.value--;
}

// 1. Criar ou Atualizar Info Básica
async function criarQuestionarioBase() {
  if (!questionarioData.value.titulo) return alert("Preencha o título.");
  
  isLoading.value = true;
  try {
    if (novoQuestionarioId.value) {
      step.value = 2;
      return;
    }

    const result = await apiService.createQuestionario(questionarioData.value);
    if (result && result.id) {
      novoQuestionarioId.value = result.id;
      step.value = 2;
    } else {
      alert('Erro ao criar questionário. Verifique os dados.');
    }
  } catch (e) {
    console.error(e);
    alert('Erro de conexão ao criar questionário.');
  } finally {
    isLoading.value = false;
  }
}

// 2. Salvar Escala
async function salvarEscala() {
  if (!novoQuestionarioId.value) return;
  isLoading.value = true;
  
  try {
    for (const opcao of escalaOpcoes.value) {
      await apiService.createOpcaoResposta(novoQuestionarioId.value, opcao);
    }
    step.value = 3;
  } catch (e) {
    console.error(e);
    alert('Erro ao salvar opções de resposta.');
  } finally {
    isLoading.value = false;
  }
}

// Funções Locais de UI
function addDimensao() {
  dimensoes.value.push({
    tempId: Date.now(),
    titulo: '',
    nomeIndicador: '',
    ordem: dimensoes.value.length + 1,
    perguntas: [{ texto: '' }] 
  });
}

function removeDimensao(index: number) {
  if(confirm("Tem certeza que deseja remover este tópico inteiro?")) {
    dimensoes.value.splice(index, 1);
  }
}

function addPergunta(dimIndex: number) {
  const dimAlvo = dimensoes.value[dimIndex];
  if (dimAlvo) {
    dimAlvo.perguntas.push({ texto: '' });
  }
}

function removePergunta(dimIndex: number, pergIndex: number) {
  const dimAlvo = dimensoes.value[dimIndex];
  if (dimAlvo) {
    dimAlvo.perguntas.splice(pergIndex, 1);
  }
}

// 3. Finalizar
async function finalizarCadastro() {
  if (!novoQuestionarioId.value) return;
  if (dimensoes.value.length === 0) return alert("Adicione pelo menos um tópico.");

  const temCampoVazio = dimensoes.value.some(d => !d.titulo || d.perguntas.some(p => !p.texto));
  if (temCampoVazio) return alert("Preencha todos os títulos de tópicos e perguntas antes de salvar.");

  isLoading.value = true;

  try {
    for (const dim of dimensoes.value) {
      const dimCriada = await apiService.createDimensao(novoQuestionarioId.value, {
        titulo: dim.titulo,
        nomeIndicador: dim.nomeIndicador || dim.titulo,
        ordem: dim.ordem
      });

      if (dimCriada && dimCriada.id) {
        for (const perg of dim.perguntas) {
          if (perg.texto.trim()) {
            await apiService.createPergunta(novoQuestionarioId.value, dimCriada.id, {
              texto: perg.texto
            });
          }
        }
      }
    }
    alert('Questionário criado com sucesso!');
    router.push('/dashboard'); // Redireciona para o Admin
  } catch (e) {
    console.error(e);
    alert('Erro ao salvar estrutura.');
  } finally {
    isLoading.value = false;
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
            <div>
              <h1 class="content-title">Criar Novo Formulário</h1>
              <p class="subtitle">Defina as perguntas e escalas para uma nova avaliação.</p>
            </div>
            
            <div class="progress-bar">
               <div class="progress-step" :class="{ active: step >= 1 }">1</div>
               <div class="progress-line"></div>
               <div class="progress-step" :class="{ active: step >= 2 }">2</div>
               <div class="progress-line"></div>
               <div class="progress-step" :class="{ active: step >= 3 }">3</div>
            </div>
          </header>

          <div v-if="step === 1" class="step-container fade-in">
            <div class="step-header">
              <h2>Informações Básicas</h2>
              <p>Configure a apresentação do questionário.</p>
            </div>

            <div class="form-group">
              <label>Título do Questionário <span class="required">*</span></label>
              <input v-model="questionarioData.titulo" type="text" placeholder="Ex: Avaliação de Riscos Psicossociais" class="input-field" />
            </div>

            <div class="form-group">
              <label>Descrição (Interna)</label>
              <textarea v-model="questionarioData.descricao" rows="2" placeholder="Descrição para controle administrativo..." class="input-field"></textarea>
            </div>

            <div class="form-group">
              <label>Texto de Introdução</label>
              <textarea v-model="questionarioData.textoIntroducao" rows="4" placeholder="Mensagem de boas-vindas ao funcionário..." class="input-field"></textarea>
            </div>

            <div class="form-group">
              <label>Termo de Consentimento</label>
              <textarea v-model="questionarioData.textoConsentimento" rows="3" placeholder="Termos legais ou LGPD..." class="input-field"></textarea>
            </div>

            <div class="actions-right">
              <button class="btn-primary" @click="criarQuestionarioBase" :disabled="isLoading">
                {{ isLoading ? 'Salvando...' : 'Próximo: Escala →' }}
              </button>
            </div>
          </div>

          <div v-if="step === 2" class="step-container fade-in">
            <div class="step-header">
              <h2>Escala de Resposta</h2>
              <p>Defina as opções (Likert) disponíveis para o usuário.</p>
            </div>

            <div class="escala-lista">
              <div v-for="(opcao, index) in escalaOpcoes" :key="index" class="opcao-card">
                <div class="drag-handle">☰</div>
                <div class="opcao-valor-badge">{{ opcao.valor }}</div>
                <input v-model="opcao.texto" placeholder="Texto da opção" class="input-clean" />
                <button class="btn-icon-remove" @click="escalaOpcoes.splice(index, 1)" title="Remover">✕</button>
              </div>
            </div>

            <button class="btn-secondary small" @click="escalaOpcoes.push({ texto: '', valor: escalaOpcoes.length + 1, ordem: escalaOpcoes.length + 1 })">
              + Adicionar Opção
            </button>

            <div class="actions-between">
              <button class="btn-outline" @click="prevStep">← Voltar</button>
              <button class="btn-primary" @click="salvarEscala" :disabled="isLoading">
                {{ isLoading ? 'Salvando...' : 'Próximo: Perguntas →' }}
              </button>
            </div>
          </div>

          <div v-if="step === 3" class="step-container fade-in">
            <div class="step-header">
              <h2>Estrutura do Questionário</h2>
              <p>Organize as perguntas dentro de Tópicos/Dimensões.</p>
            </div>

            <div v-if="dimensoes.length === 0" class="empty-state">
              <p>Nenhum tópico criado.</p>
              <button class="btn-primary" @click="addDimensao">Começar Adicionando um Tópico</button>
            </div>

            <div v-else class="dimensoes-wrapper">
              <div v-for="(dim, index) in dimensoes" :key="dim.tempId" class="dimensao-card">
                <div class="dimensao-top">
                   <div class="inputs-dimensao">
                     <input v-model="dim.titulo" class="input-titulo-dim" placeholder="Título do Tópico (Ex: Demanda de Trabalho)" />
                     <input v-model="dim.nomeIndicador" class="input-sub-dim" placeholder="Nome curto para relatório (Opcional)" />
                   </div>
                   <button class="btn-remove-dim" @click="removeDimensao(index)">Excluir Tópico</button>
                </div>

                <div class="perguntas-list">
                  <div v-for="(perg, pIndex) in dim.perguntas" :key="pIndex" class="pergunta-row">
                    <span class="bullet-p">•</span>
                    <input v-model="perg.texto" class="input-pergunta" placeholder="Digite a pergunta..." />
                    <button class="btn-icon-remove small" @click="removePergunta(index, pIndex)">✕</button>
                  </div>
                  <button class="btn-add-p" @click="addPergunta(index)">+ Nova Pergunta</button>
                </div>
              </div>

              <button class="btn-secondary dashed full-width" @click="addDimensao">
                + Adicionar Outro Tópico
              </button>
            </div>

            <div class="actions-between top-margin">
              <button class="btn-outline" @click="prevStep">← Voltar</button>
              <button class="btn-success" @click="finalizarCadastro" :disabled="isLoading">
                {{ isLoading ? 'Finalizando...' : '✅ Salvar Tudo' }}
              </button>
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
  max-width: 900px; width: 100%; background: white; padding: 2.5rem;
  border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); margin-bottom: 2rem;
}

/* --- Header & Progress --- */
.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; border-bottom: 2px solid #f3f4f6; padding-bottom: 1rem; flex-wrap: wrap; gap: 1rem; }
.content-title { font-size: 1.8rem; color: #111; margin: 0; }
.subtitle { color: #6b7280; margin: 5px 0 0 0; }

.progress-bar { display: flex; align-items: center; gap: 5px; }
.progress-step {
  width: 32px; height: 32px; border-radius: 50%; background: #e5e7eb; color: #6b7280;
  display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 0.95rem;
}
.progress-step.active { background: #2563eb; color: white; }
.progress-line { width: 30px; height: 3px; background: #e5e7eb; }

/* --- Forms & Inputs --- */
.step-header { margin-bottom: 2rem; border-left: 4px solid #2563eb; padding-left: 1rem; }
.step-header h2 { margin: 0 0 0.5rem 0; font-size: 1.4rem; color: #374151; }
.step-header p { margin: 0; color: #6b7280; font-size: 1rem; }

.form-group { margin-bottom: 1.5rem; }
label { display: block; font-weight: 600; font-size: 0.95rem; margin-bottom: 0.5rem; color: #374151; }
.required { color: #ef4444; }
.input-field {
  width: 100%; padding: 0.8rem; border: 1px solid #d1d5db; border-radius: 6px;
  font-family: inherit; font-size: 1rem; transition: border-color 0.2s; box-sizing: border-box;
}
.input-field:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1); }

/* --- Escala --- */
.opcao-card {
  display: flex; align-items: center; gap: 12px; background: #f8fafc;
  border: 1px solid #e2e8f0; padding: 1rem; border-radius: 8px; margin-bottom: 1rem;
}
.drag-handle { cursor: grab; color: #94a3b8; font-size: 1.2rem; }
.opcao-valor-badge {
  background: #dbeafe; color: #1e40af; font-weight: bold; width: 32px; height: 32px;
  border-radius: 6px; display: flex; align-items: center; justify-content: center; font-size: 0.9rem;
}
.input-clean { flex: 1; border: 1px solid transparent; background: transparent; font-size: 1rem; padding: 5px; color: #333; }
.input-clean:focus { border-bottom: 1px solid #2563eb; outline: none; }

/* --- Dimensões --- */
.dimensao-card {
  background: #ffffff; border: 1px solid #e5e7eb; border-radius: 8px;
  padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
.dimensao-top { display: flex; justify-content: space-between; align-items: flex-start; gap: 15px; margin-bottom: 1.2rem; border-bottom: 1px solid #f3f4f6; padding-bottom: 1rem; }
.inputs-dimensao { flex: 1; }
.input-titulo-dim { font-weight: 700; font-size: 1.1rem; border: 1px solid transparent; padding: 0.5rem; background: #f3f4f6; width: 100%; border-radius: 4px; box-sizing: border-box; }
.input-titulo-dim:focus { background: white; border-color: #2563eb; outline: none; }
.input-sub-dim { margin-top: 8px; font-size: 0.9rem; border: none; border-bottom: 1px dashed #cbd5e1; padding: 4px 0; width: 100%; color: #64748b; }

.perguntas-list { border-left: 2px solid #e5e7eb; padding-left: 1.5rem; margin-top: 1rem; }
.pergunta-row { display: flex; align-items: center; gap: 10px; margin-bottom: 0.8rem; }
.bullet-p { color: #cbd5e1; font-size: 1.5rem; line-height: 0; }
.input-pergunta { flex: 1; padding: 0.6rem; font-size: 0.95rem; border: 1px solid #e5e7eb; border-radius: 4px; box-sizing: border-box; }
.input-pergunta:focus { border-color: #2563eb; outline: none; }

.btn-remove-dim { font-size: 0.85rem; color: #ef4444; background: none; border: none; cursor: pointer; font-weight: 600; }
.btn-remove-dim:hover { text-decoration: underline; }

/* --- Botões --- */
.btn-primary { background: #2563eb; color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; font-size: 1rem; }
.btn-primary:hover { background: #1d4ed8; }
.btn-primary:disabled { background: #93c5fd; cursor: not-allowed; }

.btn-secondary { background: #f3f4f6; color: #374151; border: 1px solid #e5e7eb; padding: 0.6rem 1rem; border-radius: 6px; font-weight: 500; cursor: pointer; transition: all 0.2s; }
.btn-secondary:hover { background: #e5e7eb; }
.dashed { border-style: dashed; background: white; width: 100%; padding: 1rem; color: #6b7280; }
.dashed:hover { border-color: #2563eb; color: #2563eb; }

.btn-outline { background: white; border: 1px solid #d1d5db; color: #4b5563; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; font-size: 1rem; transition: all 0.2s; }
.btn-outline:hover { border-color: #9ca3af; color: #111; background: #f9fafb; }

.btn-success { background: #10b981; color: white; border: none; padding: 0.8rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; font-size: 1rem; }
.btn-success:hover { background: #059669; }

.btn-icon-remove { background: #fee2e2; color: #dc2626; border: none; width: 32px; height: 32px; border-radius: 6px; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: background 0.2s; }
.btn-icon-remove:hover { background: #fecaca; }
.btn-icon-remove.small { width: 28px; height: 28px; font-size: 0.8rem; }

.btn-add-p { background: none; border: none; color: #2563eb; font-size: 0.9rem; font-weight: 600; margin-top: 0.5rem; padding: 0; cursor: pointer; }
.btn-add-p:hover { text-decoration: underline; }

.actions-right { display: flex; justify-content: flex-end; margin-top: 2rem; }
.actions-between { display: flex; justify-content: space-between; margin-top: 2rem; }
.top-margin { margin-top: 2rem; }

/* --- Util --- */
.empty-state { text-align: center; padding: 4rem; background: #f9fafb; border-radius: 8px; border: 2px dashed #e5e7eb; margin-bottom: 2rem; }
.fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }

/* Responsivo */
@media (max-width: 768px) {
  .app-layout { flex-direction: column; overflow: auto; }
  .sidebar { width: 100%; height: auto; border-right: none; border-bottom: 1px solid #e5e7eb; padding: 1rem; }
  .main-wrapper { height: auto; overflow-y: visible; }
  .content-wrapper { padding: 1.5rem; }
}
</style>