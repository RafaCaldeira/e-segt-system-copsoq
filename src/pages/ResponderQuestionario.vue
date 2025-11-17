<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { apiService } from '../services/api.service';
// Importamos o novo tipo de opção
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
const respostas = ref<Record<number, number | null>>({}); // Pode ser nulo no início
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

    // *** 1. INICIALIZAR AS RESPOSTAS ***
    // (Para garantir que a validação 'todas respondidas' funcione)
    if (questionario.value) {
      const todasPerguntas = questionario.value.dimensoes.flatMap(d => d.perguntas);
      for (const p of todasPerguntas) {
        respostas.value[p.id] = null; // Começa como nulo
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

// *** 2. OPÇÕES DE RESPOSTA DINÂMICAS ***
// Pega a lista de opções (ex: 1-5 ou 0-6) que veio da API
const opcoesRespostaDinamicas = computed<OpcaoRespostaDto[]>(() => {
  if (!questionario.value) return [];
  // Ordena pela 'Ordem' (ex: 1º, 2º, 3º...)
  return questionario.value.opcoesResposta.sort((a, b) => a.ordem - b.ordem);
});

const totalPaginas = computed(() => (questionario.value?.dimensoes.length ?? 0) + 1);

// --- Lógica de Ações (Funções) ---
async function irParaProximaPagina() {
  if (!questionario.value || isSubmitting.value) return;

  // (Validação Página 0 - Consentimento)
  if (paginaAtual.value === 0 && !consentimentoAceito.value) {
    alert('Você precisa aceitar o termo de consentimento para continuar.');
    return;
  }

  // (Validação Página 1 - CPF)
  if (paginaAtual.value === 1) {
    if (!cpfInput.value || cpfInput.value.replace(/\D/g, '').length !== 11) { 
       alert('Por favor, preencha o seu CPF corretamente (11 números).');
       return;
    }
  }

  // (Validação Páginas de Perguntas)
  if (dimensaoAtual.value) {
    for (const pergunta of dimensaoAtual.value.perguntas) {
      if (respostas.value[pergunta.id] === null) { // Verifica se é nulo
        alert('Por favor, responda a todas as perguntas desta página.');
        return;
      }
    }
  }
  
  // (Navegação)
  const totalPaginasQuestionario = (questionario.value.dimensoes.length || 0) + 1;
  if (paginaAtual.value < totalPaginasQuestionario) {
    paginaAtual.value++;
    return;
  }
  
  // (Submit na última página)
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

  // Validação final (vê se alguma resposta ainda é 'null')
  const respostasInvalidas = Object.values(respostas.value).some(v => v === null);
  if (respostasInvalidas) {
      errorMessage.value = 'Parece que algumas perguntas não foram respondidas. Por favor, volte e verifique.';
      isSubmitting.value = false;
      return;
  }

  // Mapeia as respostas para o DTO
  const respostasArray: RespostaDto[] = Object.keys(respostas.value).map(perguntaIdStr => {
    const perguntaId = parseInt(perguntaIdStr, 10);
    return {
      perguntaId: perguntaId,
      valorResposta: respostas.value[perguntaId]! // O '!' diz ao TS que temos a certeza que não é nulo
    };
  });

  const submissaoDto: SubmissaoDto = {
    cpf: cpfInput.value.replace(/\D/g, ''), 
    respostas: respostasArray
  };

  // Envia para a API
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
            <div class="progresso">
              Página 1 / {{ totalPaginas }}
            </div>
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
                
                <div class="opcoes-likert">
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
            
            <span style="flex-grow: 1;"></span> <button 
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
/* O seu CSS existente está ótimo, não precisa de alterações */
/* ... (Cole o seu CSS completo aqui) ... */
:global(body) {
  margin: 0;
  background-color: #333;
  color: #333;
  font-family: Arial, sans-serif;
}
.page-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  align-items: center;
}
.page-header {
  width: 100%;
  padding: 1rem 0;
  background-color: #ffffff;
  text-align: center;
  border-bottom: 1px solid #e0e0e0;
}
.header-logo {
  width: 150px;
  height: auto;
}
.main-content {
  width: 100%;
  flex: 1;
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  box-sizing: border-box; 
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
}
.opcoes-likert {
  display: flex;
  flex-wrap: wrap; /* Permite que as opções quebrem a linha */
  gap: 1rem; /* Espaço entre os botões */
  margin-top: 0.5rem;
}
.opcoes-likert label {
  margin: 0;
  cursor: pointer;
  color: #444;
  display: flex; /* Alinha o input e o texto */
  align-items: center;
}
.opcoes-likert input {
  margin-right: 0.3rem;
  cursor: pointer;
}
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
  background-color: #3b82f6;
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