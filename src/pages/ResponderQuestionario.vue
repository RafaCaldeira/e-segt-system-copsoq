<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { apiService } from '../services/api.service';
import type { QuestionarioParaResponderDto, DimensaoRespostaDto, OpcaoRespostaDto } from '../types/questionario.types';
import type { RespostaDto } from '../types/resposta.types';
import type { SubmissaoDto } from '../types/submissao.types';

// --- Estado do Componente ---
const questionario = ref<QuestionarioParaResponderDto | null>(null);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);
const paginaAtual = ref(0); 
const consentimentoAceito = ref(false);
const cpfInput = ref(''); 

// Estado das Respostas
const respostas = ref<Record<number, number | null>>({}); // Para Likert (Números)
const respostasTexto = ref<Record<number, string>>({});   // Para Perguntas Abertas (Texto)

const isSubmitting = ref(false);
const submitSuccess = ref(false);

const route = useRoute();
const token = route.params.token as string;

// --- Busca de Dados (onMounted) ---
onMounted(async () => {
  if (!token) {
    errorMessage.value = 'Token de acesso inválido ou ausente.';
    isLoading.value = false;
    return;
  }
  
  isLoading.value = true;
  errorMessage.value = null;
  const data = await apiService.getQuestionarioParaResponder(token);

  if (data) {
    // Limpeza dos $id e $ref (JSON.NET)
    const cleanedData = JSON.parse(JSON.stringify(data), (key, value) => {
      if (value && typeof value === 'object' && value.$values) return value.$values;
      if (key === '$id' || key === '$ref') return undefined;
      return value;
    });
    
    questionario.value = cleanedData as QuestionarioParaResponderDto;
    
    // Pré-preenche o CPF
    if (questionario.value?.funcionario?.cpf) {
      cpfInput.value = questionario.value.funcionario.cpf;
    }

    // *** INICIALIZAR AS RESPOSTAS ***
    if (questionario.value) {
      const todasPerguntas = questionario.value.dimensoes.flatMap(d => d.perguntas);
      for (const p of todasPerguntas) {
        // Inicializa ambos como vazios
        respostas.value[p.id] = null; 
        respostasTexto.value[p.id] = '';
      }
    }

  } else {
    errorMessage.value = 'Não foi possível carregar o questionário. Verifique o link ou tente novamente.';
  }
  isLoading.value = false;
});

// --- Lógica de Visualização (Computed) ---
const dimensaoAtual = computed<DimensaoRespostaDto | null>(() => {
  const dimensaoIndex = paginaAtual.value - 2; 
  if (!questionario.value || paginaAtual.value < 2 || dimensaoIndex >= questionario.value.dimensoes.length) {
    return null;
  }
  return questionario.value.dimensoes[dimensaoIndex] ?? null;
});

const opcoesRespostaDinamicas = computed<OpcaoRespostaDto[]>(() => {
  if (!questionario.value) return [];
  return questionario.value.opcoesResposta.sort((a, b) => a.ordem - b.ordem);
});

const totalPaginas = computed(() => (questionario.value?.dimensoes.length ?? 0) + 1);

// --- FUNÇÃO AUXILIAR: Decidir se é Texto ou Likert ---
function ehPerguntaTexto(): boolean {
    // LÓGICA: Se a dimensão atual tiver "Qualitativa" ou "Aberta" no título,
    // OU se o questionário não tiver opções globais cadastradas.
    // Você pode ajustar isso conforme sua necessidade.
    
    const tituloDimensao = dimensaoAtual.value?.titulo?.toLowerCase() || '';
    if (tituloDimensao.includes('qualitativa') || tituloDimensao.includes('obs') || tituloDimensao.includes('comentário')) {
        return true;
    }

    // Fallback: Se não tem opções de resposta (bolinhas), assume que é texto
    if (opcoesRespostaDinamicas.value.length === 0) {
        return true;
    }

    return false;
}

// --- Lógica de Ações (Funções) ---
async function irParaProximaPagina() {
  if (!questionario.value || isSubmitting.value) return;

  // Pag 0: Consentimento
  if (paginaAtual.value === 0 && !consentimentoAceito.value) {
    alert('Você precisa aceitar o termo de consentimento para continuar.');
    return;
  }

  // Pag 1: CPF
  if (paginaAtual.value === 1) {
    if (!cpfInput.value || cpfInput.value.replace(/\D/g, '').length !== 11) { 
       alert('Por favor, preencha o seu CPF corretamente (11 números).');
       return;
    }
  }

  // Pag 2+: Perguntas (Validação Inteligente)
  if (dimensaoAtual.value) {
    for (const pergunta of dimensaoAtual.value.perguntas) {
        
      if (ehPerguntaTexto()) {
          // Validação para Texto: Se for obrigatório (ajuste se quiser opcional)
          // Se quiser obrigar escrever algo:
          /*
          if (!respostasTexto.value[pergunta.id] || respostasTexto.value[pergunta.id].trim() === '') {
             alert('Por favor, responda a pergunta: ' + pergunta.texto);
             return;
          }
          */
          // Se for opcional, não faz nada aqui.
      } else {
          // Validação para Likert: Tem que ter marcado algo
          if (respostas.value[pergunta.id] === null) { 
            alert('Por favor, responda todas as perguntas desta página.');
            return;
          }
      }
    }
  }
  
  // Navegação
  const totalPaginasQuestionario = (questionario.value.dimensoes.length || 0) + 1;
  if (paginaAtual.value < totalPaginasQuestionario) {
    paginaAtual.value++;
    return;
  }
  
  if (paginaAtual.value === totalPaginasQuestionario) {
    await handleSubmit();
  }
}

function irParaPaginaAnterior() {
  if (paginaAtual.value > 0) {
    paginaAtual.value--;
  }
}

async function handleSubmit() {
  if (isSubmitting.value || !questionario.value) return;
  isSubmitting.value = true;
  errorMessage.value = null;

  // --- MONTAGEM DO DTO ---
  const respostasArray: RespostaDto[] = [];

  // 1. Adiciona Likert (Números)
  Object.keys(respostas.value).forEach(key => {
    const id = parseInt(key);
    const valor = respostas.value[id];
    
    // Verificação rigorosa do TypeScript:
    // Só adiciona se for especificamente um número (evita null e undefined)
    if (typeof valor === 'number') {
        respostasArray.push({ 
            perguntaId: id, 
            valorResposta: valor 
        });
    }
  });

  // 2. Adiciona Texto (Abertas)
  Object.keys(respostasTexto.value).forEach(key => {
    const id = parseInt(key);
    const texto = respostasTexto.value[id];
    
    if (texto && texto.trim().length > 0) {
        respostasArray.push({ 
            perguntaId: id, 
            textoResposta: texto,
            valorResposta: null // Agora o TypeScript vai aceitar esse null!
        });
    }
  });

  // 2. Adiciona Texto (Abertas)
  Object.keys(respostasTexto.value).forEach(key => {
    const id = parseInt(key);
    const texto = respostasTexto.value[id];
    if (texto && texto.trim().length > 0) {
        respostasArray.push({ 
            perguntaId: id, 
            textoResposta: texto,
            valorResposta: null // Backend agora aceita null aqui
        });
    }
  });

  // Se não tiver nenhuma resposta (nem texto nem numero), bloqueia
  if (respostasArray.length === 0) {
      errorMessage.value = "Você não preencheu nenhuma resposta.";
      isSubmitting.value = false;
      return;
  }

  const submissaoDto: SubmissaoDto = {
    cpf: cpfInput.value.replace(/\D/g, ''), 
    respostas: respostasArray
  };

  try {
    const result = await apiService.submitRespostas(token, submissaoDto);

    if (result.success) {
        submitSuccess.value = true;
    } else {
        errorMessage.value = result.message || 'Erro desconhecido ao enviar.';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  } catch (error) {
    errorMessage.value = 'Ocorreu um erro inesperado na comunicação com o servidor.';
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  isSubmitting.value = false;
}
</script>

<template>
  <div class="page-wrapper">
    
    <header class="page-header">
      <img src="../assets/e-segt.png" alt="E-SegT Logo" class="header-logo">
    </header>

    <main class="main-content">
      <div class="responder-container">
        
        <div v-if="isLoading" class="loading">
          Carregando...
        </div>

        <div v-else-if="errorMessage" class="error-message">
          {{ errorMessage }}
        </div>

        <div v-else-if="submitSuccess" class="success-message">
          <h2>Obrigado por participar!</h2>
          <p>As suas respostas foram enviadas com sucesso.</p>
        </div>

        <div v-else-if="questionario">

          <div v-if="paginaAtual === 0">
             <h1 class="content-title">{{ questionario.titulo }}</h1>
             <p v-html="questionario.textoIntroducao.replace(/\n/g, '<br>')"></p>
             <h3>Termo de consentimento</h3>
             <p v-html="questionario.textoConsentimento.replace(/\n/g, '<br>')"></p>
             <div class="consentimento">
               <input type="checkbox" id="consentimento" v-model="consentimentoAceito">
               <label for="consentimento">Li e estou de acordo em participar voluntariamente desta avaliação.</label>
             </div>
          </div>

          <div v-else-if="paginaAtual === 1" class="dados-container">
            <div class="progresso">Página 1 / {{ totalPaginas }}</div>
            <h1 class="content-title">Confirmação de Dados</h1>
            <p>Por favor, confirme os seus dados e preencha o seu CPF.</p>
            <div class="dados-info"><strong>Nome:</strong> {{ questionario.funcionario.nome }}</div>
            <div class="dados-info"><strong>Empresa:</strong> {{ questionario.funcionario.nomeEmpresa }}</div>
            <div class="dados-info"><strong>Setor:</strong> {{ questionario.funcionario.setor }}</div>
            <div class="form-group-cpf">
              <label for="cpf">CPF (apenas números):</label>
              <input type="text" id="cpf" v-model="cpfInput" maxlength="11" placeholder="00011122233">
            </div>
          </div>

          <div v-else-if="dimensaoAtual">
            <div class="progresso">
              Página {{ paginaAtual - 1 }} / {{ questionario.dimensoes.length }}
            </div>
            <h1 class="content-title">{{ dimensaoAtual.titulo }}</h1>
            
            <form @submit.prevent="irParaProximaPagina">
              <div v-for="pergunta in dimensaoAtual.perguntas" :key="pergunta.id" class="pergunta-item">
                <p>{{ pergunta.texto }}</p>
                
                <div v-if="ehPerguntaTexto()" class="campo-texto">
                    <textarea 
                        v-model="respostasTexto[pergunta.id]"
                        class="textarea-resposta"
                        placeholder="Digite sua resposta aqui..."
                        rows="4"
                    ></textarea>
                </div>

                <div v-else class="opcoes-likert">
                  <label v-for="opcao in opcoesRespostaDinamicas" :key="opcao.valor">
                    <input 
                      type="radio" 
                      :name="'pergunta-' + pergunta.id" 
                      :value="opcao.valor" 
                      v-model="respostas[pergunta.id]">
                    {{ opcao.texto }}
                  </label>
                </div>

              </div>
            </form>
          </div>

          <div class="navegacao">
            <button 
              class="btn-voltar"
              v-if="paginaAtual > 0" 
              @click="irParaPaginaAnterior" 
              :disabled="isSubmitting">
              Voltar
            </button>
            
            <span style="flex-grow: 1;"></span> 

            <button 
              class="btn-continuar"
              @click="irParaProximaPagina" 
              :disabled="(paginaAtual === 0 && !consentimentoAceito) || (paginaAtual === 1 && cpfInput.replace(/\D/g, '').length !== 11) || isSubmitting">
              {{ isSubmitting ? 'A enviar...' : (paginaAtual === totalPaginas ? 'Finalizar' : 'Continuar') }}
            </button>
          </div>

        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* (MANTIVE SEUS ESTILOS ORIGINAIS + ESTILO DO TEXTAREA) */
:global(html), :global(body), :global(#app) {
  height: auto !important;
  min-height: 100% !important;
  overflow-y: auto !important;
  overflow-x: hidden;
  background-color: #333;
}

.textarea-resposta {
    width: 100%;
    padding: 12px;
    border: 1px solid #ccc;
    border-radius: 6px;
    font-family: inherit;
    font-size: 1rem;
    resize: vertical;
    margin-top: 5px;
    background-color: #fff;
}

.textarea-resposta:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

.page-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  width: 100%;
  align-items: center;
  background-color: #333;
}
.page-header {
  width: 100%;
  padding: 1rem 0;
  background-color: #ffffff;
  text-align: center;
  border-bottom: 1px solid #e0e0e0;
  flex-shrink: 0;
}
.header-logo {
  width: 150px;
  height: auto;
}
.main-content {
  width: 100%;
  flex: 1;
  padding: 2rem 1rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  box-sizing: border-box;
}
.responder-container {
  max-width: 900px;
  width: 100%;
  padding: 2.5rem 3rem;
  border-radius: 8px;
  background-color: #f4f7f6;
  color: #333;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  margin-bottom: 2rem;
}
@media (max-width: 600px) {
  .responder-container { padding: 1.5rem; }
  .main-content { padding: 1rem; }
}
.loading, .error-message, .success-message {
  text-align: center;
  padding: 3rem;
  font-size: 1.2rem;
}
.error-message { color: #d9534f; }
.success-message { color: #5cb85c; }
h1.content-title {
  font-size: 2.2rem;
  color: #333;
  border-bottom: 4px solid #3b82f6;
  padding-bottom: 0.5rem;
  margin-bottom: 2rem;
  display: inline-block;
  line-height: 1.2;
}
h3 {
  font-size: 1.3rem;
  font-weight: bold;
  color: #222;
  margin-top: 2rem;
  margin-bottom: 1rem;
}
p, li {
  font-size: 1rem;
  line-height: 1.6;
  color: #444;
}
.consentimento {
  margin-top: 1.5rem;
  display: flex;
  align-items: flex-start;
}
.consentimento input {
  width: 18px;
  height: 18px;
  margin-right: 0.8rem;
  cursor: pointer;
  margin-top: 4px;
}
.consentimento label {
  font-size: 1.1rem;
  color: #333;
  cursor: pointer;
}
.dados-container { padding: 1rem 0; }
.dados-info {
  font-size: 1.1rem;
  margin: 1rem 0;
  padding: 1rem;
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 4px;
}
.form-group-cpf { margin-top: 1.5rem; }
.form-group-cpf label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: bold;
}
.form-group-cpf input {
  width: 100%;
  padding: 0.8rem;
  font-size: 1rem;
  border-radius: 4px;
  border: 1px solid #ccc;
  box-sizing: border-box; 
}
.progresso {
  text-align: right;
  color: #777;
  margin-bottom: 1rem;
  font-weight: bold;
}
.pergunta-item {
  margin-bottom: 1.5rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px dashed #ccc;
}
.pergunta-item p {
  font-size: 1.2rem;
  font-weight: 500;
  color: #222;
  margin-bottom: 1rem;
}
.opcoes-likert {
  display: flex;
  flex-wrap: wrap; 
  gap: 1rem; 
  margin-top: 0.5rem;
}
.opcoes-likert label {
  margin: 0;
  cursor: pointer;
  color: #444;
  display: flex;
  align-items: center;
  background: #fff;
  padding: 0.5rem 0.8rem;
  border: 1px solid #e0e0e0;
  border-radius: 20px;
  transition: all 0.2s;
}
.opcoes-likert label:hover {
  background: #eff6ff;
  border-color: #3b82f6;
}
.opcoes-likert input:checked + span {
   font-weight: bold;
}
.opcoes-likert input {
  margin-right: 0.5rem;
  cursor: pointer;
}
.navegacao {
  margin-top: 2.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}
.navegacao button {
  padding: 0.8rem 1.5rem;
  cursor: pointer;
  border: none;
  border-radius: 6px;
  font-weight: bold;
  font-size: 1rem;
  transition: background-color 0.2s;
  min-width: 120px;
}
.btn-voltar {
  background-color: #64748b;
  color: white;
}
.btn-voltar:hover { background-color: #475569; }
.btn-continuar {
  background-color: #3b82f6;
  color: white;
}
.btn-continuar:hover { background-color: #2563eb; }
.navegacao button:disabled {
  background-color: #cbd5e1;
  cursor: not-allowed;
  opacity: 0.7;
}
</style>