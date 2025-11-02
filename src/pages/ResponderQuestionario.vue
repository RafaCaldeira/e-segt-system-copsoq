<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { apiService } from '../services/api.service';
import type { QuestionarioParaResponderDto, DimensaoRespostaDto } from '../types/questionario.types';
import type { RespostaDto } from '../types/resposta.types';
import type { SubmissaoDto } from '../types/submissao.types';

// --- Estado do Componente ---
// (O seu script setup está 100% correto, por isso permanece igual)
const questionario = ref<QuestionarioParaResponderDto | null>(null);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);
const paginaAtual = ref(0); 
const consentimentoAceito = ref(false);
const cpfInput = ref(''); 
const respostas = ref<Record<number, number>>({});
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
    const cleanedData = JSON.parse(JSON.stringify(data), (key, value) => {
      if (value && typeof value === 'object' && value.$values) return value.$values;
      if (key === '$id' || key === '$ref') return undefined;
      return value;
    });
    questionario.value = cleanedData as QuestionarioParaResponderDto;
    if (questionario.value?.funcionario?.cpf) {
      cpfInput.value = questionario.value.funcionario.cpf;
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
const totalPaginas = computed(() => (questionario.value?.dimensoes.length ?? 0) + 1);

// --- Lógica de Ações (Funções) ---
async function irParaProximaPagina() {
  if (!questionario.value || isSubmitting.value) return;
  if (paginaAtual.value === 0 && !consentimentoAceito.value) {
    alert('Você precisa aceitar o termo de consentimento para continuar.');
    return;
  }
  if (paginaAtual.value === 1) {
    if (!cpfInput.value || cpfInput.value.replace(/\D/g, '').length !== 11) { 
       alert('Por favor, preencha o seu CPF corretamente (11 números).');
       return;
    }
  }
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
  const todasPerguntasIds = questionario.value.dimensoes.flatMap(d => d.perguntas).map(p => p.id);
  for (const id of todasPerguntasIds) {
    const resposta = respostas.value[id];
    if (resposta === undefined || resposta === null) {
      errorMessage.value = 'Por favor, responda a todas as perguntas antes de finalizar.';
      isSubmitting.value = false;
      return;
    }
  }
  const respostasArray: RespostaDto[] = Object.keys(respostas.value).map(perguntaIdStr => {
    const perguntaId = parseInt(perguntaIdStr, 10);
    return {
      perguntaId: perguntaId,
      valorResposta: respostas.value[perguntaId]! 
    };
  });
  const submissaoDto: SubmissaoDto = {
    cpf: cpfInput.value.replace(/\D/g, ''), 
    respostas: respostasArray
  };
  const success = await apiService.submitRespostas(token, submissaoDto);
  if (success) {
    submitSuccess.value = true;
  } else {
    errorMessage.value = 'Erro ao enviar as suas respostas. Por favor, tente novamente.';
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
            <h1 class="content-title">Questionário COPSOQ</h1>
            
            <h2>Bem-vindo(a) à Avaliação COPSOQ</h2>
            <p v-html="questionario.textoIntroducao.replace(/\n/g, '<br>')"></p>
            
            <h3>Objetivo</h3>
            <ul>
              <li>Carga de trabalho e ritmo;</li>
              <li>Apoio social e liderança;</li>
              <li>Reconhecimento e equilíbrio entre vida pessoal e profissional.</li>
            </ul>
            <p>As respostas serão utilizadas apenas para fins de diagnóstico...</p>
            
            <div class="confidencial-lock">
              <span class="icon-lock"></span> <p>Todas as informações coletadas são confidenciais e analisadas de forma anônima.</p>
            </div>

            <h3>Termo de consentimento</h3>
            <p v-html="questionario.textoConsentimento.replace(/\n/g, '<br>')"></p>
            
            <div class="consentimento">
              <input type="checkbox" id="consentimento" v-model="consentimentoAceito">
              <label for="consentimento">Li e estou de acordo em participar voluntariamente desta avaliação.</label>
            </div>
          </div>

          <div v-else-if="paginaAtual === 1" class="dados-container">
            <div class="progresso">
              Página 1 / {{ totalPaginas }}
            </div>
            <h1 class="content-title">Confirmação de Dados</h1>
            <p>Por favor, confirme os seus dados e preencha o seu CPF.</p>
            
            <div class="dados-info">
              <strong>Nome:</strong> {{ questionario.funcionario.nome }}
            </div>
            <div class="dados-info">
              <strong>Empresa:</strong> {{ questionario.funcionario.nomeEmpresa }}
            </div>
            <div class="dados-info">
              <strong>Setor:</strong> {{ questionario.funcionario.setor }}
            </div>
            
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
                <div class="opcoes-likert">
                  <label><input type="radio" :name="'pergunta-' + pergunta.id" :value="4" v-model="respostas[pergunta.id]"> Sempre</label>
                  <label><input type="radio" :name="'pergunta-' + pergunta.id" :value="3" v-model="respostas[pergunta.id]"> Frequentemente</label>
                  <label><input type="radio" :name="'pergunta-' + pergunta.id" :value="2" v-model="respostas[pergunta.id]"> Às vezes</label>
                  <label><input type="radio" :name="'pergunta-' + pergunta.id" :value="1" v-model="respostas[pergunta.id]"> Raramente</label>
                  <label><input type="radio" :name="'pergunta-' + pergunta.id" :value="0" v-model="respostas[pergunta.id]"> Nunca</label>
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
            
            <span style="flex-grow: 1;"></span> <button 
              class="btn-continuar"
              @click="irParaProximaPagina" 
              :disabled="(paginaAtual === 0 && !consentimentoAceito) || (paginaAtual === 1 && cpfInput.replace(/\D/g, '').length !== 11) || isSubmitting">
              {{ isSubmitting ? 'A enviar...' : (paginaAtual === totalPaginas ? 'Finalizar' : 'Continue') }}
            </button>
          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* Fundo global escuro para a página inteira */
:global(body) {
  margin: 0;
  background-color: #333;
  color: #333;
  font-family: Arial, sans-serif;
}

/* O Layout Principal (Centralizado) */
.page-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  align-items: center; /* Centraliza o conteúdo */
}

/* --- 1. Cabeçalho com o Logo --- */
.page-header {
  width: 100%;
  padding: 1rem 0;
  background-color: #ffffff; /* Fundo branco para o cabeçalho */
  text-align: center;
  border-bottom: 1px solid #e0e0e0;
}
.header-logo {
  width: 150px;
  height: auto;
}

/* --- 2. Área de Conteúdo Principal --- */
.main-content {
  width: 100%;
  flex: 1;
  padding: 2rem; /* Espaçamento em volta do card */
  display: flex;
  justify-content: center;
  align-items: flex-start; /* Alinha no topo */
  box-sizing: border-box; /* Inclui o padding na largura */
}

/* O "Card" do Questionário (Mantém o estilo do seu esboço) */
.responder-container {
  max-width: 900px;
  width: 100%;
  margin: 0;
  padding: 2.5rem 3rem;
  border-radius: 8px;
  background-color: #f4f7f6; /* Fundo cinzento claro do card */
  color: #333; /* Texto escuro */
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

/* Estados de Carregamento/Erro/Sucesso (dentro do card) */
.loading, .error-message, .success-message {
  text-align: center;
  padding: 3rem;
  font-size: 1.2rem;
}
.error-message { color: #d9534f; }
.success-message { color: #5cb85c; }

/* --- Estilos do Conteúdo (baseado no seu esboço) --- */
h1.content-title {
  font-size: 2.2rem;
  color: #333;
  border-bottom: 4px solid #3b82f6; /* Azul do seu esboço */
  padding-bottom: 0.5rem;
  margin-bottom: 2rem;
  display: inline-block;
}
h2 {
  font-size: 1.6rem;
  font-weight: bold;
  color: #111;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
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
ul { margin-bottom: 1.5rem; }
hr {
  border: none;
  border-top: 1px solid #ddd;
  margin: 2rem 0;
}

/* Ícone de Cadeado Simulado */
.confidencial-lock {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  background-color: #e9e9e9;
  padding: 1rem;
  border-radius: 6px;
  margin-top: 1.5rem;
}
.confidencial-lock .icon-lock {
  display: inline-block;
  width: 16px;
  height: 16px;
  background-color: #888;
}
.confidencial-lock p {
  margin: 0;
  color: #555;
  font-weight: 500;
}

/* Consentimento */
.consentimento {
  margin-top: 1.5rem;
  display: flex;
  align-items: center;
}
.consentimento input {
  width: 18px;
  height: 18px;
  margin-right: 0.8rem;
  cursor: pointer;
}
.consentimento label {
  font-size: 1.1rem;
  color: #333;
  cursor: pointer;
}

/* Página de Dados do Funcionário */
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

/* Página de Perguntas */
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
}
.opcoes-likert label {
  margin: 0 1rem 0.5rem 0;
  cursor: pointer;
  color: #444;
  display: inline-block;
}
.opcoes-likert input {
  margin-right: 0.3rem;
  cursor: pointer;
}

/* Navegação (Botões) - Ajustado para alinhar o 'Continue' à direita */
.navegacao {
  margin-top: 2.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.navegacao button {
  padding: 0.8rem 1.5rem;
  cursor: pointer;
  border: none;
  border-radius: 6px;
  font-weight: bold;
  font-size: 1rem;
  transition: background-color 0.2s;
}
.btn-voltar {
  background-color: #777;
  color: white;
}
.btn-continuar {
  background-color: #3b82f6; /* Azul */
  color: white;
}
.navegacao button:hover:not(:disabled) {
  opacity: 0.8;
}
.navegacao button:disabled {
  background-color: #aaa;
  cursor: not-allowed;
  opacity: 0.7;
}

</style>