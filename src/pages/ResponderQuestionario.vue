<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router'; // Para pegar o token da URL
import { apiService } from '../services/api.service';
import type { QuestionarioParaResponderDto, DimensaoRespostaDto } from '../types/questionario.types';

// Estado do componente
const questionario = ref<QuestionarioParaResponderDto | null>(null);
const isLoading = ref<boolean>(true);
const errorMessage = ref<string | null>(null);
const paginaAtual = ref<number>(0); // 0 = Introdução, 1 = Dimensão 1, etc.
const consentimentoAceito = ref<boolean>(false);

// Pegar o token da rota
const route = useRoute();
const token = route.params.token as string; // O token da URL (ex: /responder/guid-aqui)

// Função para buscar os dados quando o componente montar
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
    questionario.value = data;
    // Opcional: Tratar os $id e $ref aqui se necessário
    // Por exemplo, limpar os dados antes de atribuir a questionario.value
  } else {
    errorMessage.value = 'Não foi possível carregar o questionário. Verifique o link ou tente novamente.';
  }
  isLoading.value = false;
});

// --- Lógica de Navegação e Exibição ---

// Computada para saber qual dimensão mostrar
const dimensaoAtual = computed<DimensaoRespostaDto | null>(() => {
  if (!questionario.value || paginaAtual.value === 0 || paginaAtual.value > questionario.value.dimensoes.length) {
    return null;
  }
  // As dimensões estão ordenadas pela API, pegamos pelo índice (paginaAtual - 1)
  return questionario.value.dimensoes[paginaAtual.value - 1] ?? null;
});

// Funções para navegar
function irParaProximaPagina() {
  if (!questionario.value) return;

  if (paginaAtual.value === 0 && !consentimentoAceito.value) {
    alert('Você precisa aceitar o termo de consentimento para continuar.');
    return;
  }

  if (paginaAtual.value < questionario.value.dimensoes.length) {
    paginaAtual.value++;
  } else {
    // Chegou ao fim das dimensões, ir para a tela de resumo/envio
    // (Lógica a ser adicionada depois)
    console.log('Chegou ao fim das perguntas. Próximo passo: Enviar.');
  }
}

function irParaPaginaAnterior() {
  if (paginaAtual.value > 0) {
    paginaAtual.value--;
  }
}

</script>

<template>
  <div class="responder-container">
    <div v-if="isLoading" class="loading">
      Carregando questionário...
    </div>

    <div v-else-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <div v-else-if="questionario">
      <h1>{{ questionario.titulo }}</h1>

      <div v-if="paginaAtual === 0">
        <h2>Bem-vindo(a)!</h2>
        <p v-html="questionario.textoIntroducao.replace(/\n/g, '<br>')"></p> <hr>
        <h3>Termo de Consentimento</h3>
        <p v-html="questionario.textoConsentimento.replace(/\n/g, '<br>')"></p>
        <div class="consentimento">
          <input type="checkbox" id="consentimento" v-model="consentimentoAceito">
          <label for="consentimento">Li e estou de acordo em participar.</label>
        </div>
      </div>

      <div v-else-if="dimensaoAtual">
        <div class="progresso">
          Página {{ paginaAtual }} / {{ questionario.dimensoes.length }}
        </div>
        <h2>{{ dimensaoAtual.titulo }}</h2>
        <form @submit.prevent="irParaProximaPagina">
          <div v-for="pergunta in dimensaoAtual.perguntas" :key="pergunta.id" class="pergunta-item">
            <p>{{ pergunta.texto }}</p>
            <div class="opcoes-likert">
              <label><input type="radio" :name="'pergunta-' + pergunta.id" value="4"> Sempre</label>
              <label><input type="radio" :name="'pergunta-' + pergunta.id" value="3"> Frequentemente</label>
              <label><input type="radio" :name="'pergunta-' + pergunta.id" value="2"> Às vezes</label>
              <label><input type="radio" :name="'pergunta-' + pergunta.id" value="1"> Raramente</label>
              <label><input type="radio" :name="'pergunta-' + pergunta.id" value="0"> Nunca</label>
              </div>
          </div>
        </form>
      </div>

      <div class="navegacao">
        <button v-if="paginaAtual > 0" @click="irParaPaginaAnterior">Voltar</button>
        <button @click="irParaProximaPagina" :disabled="paginaAtual === 0 && !consentimentoAceito">
          {{ paginaAtual === 0 ? 'Continuar' : (paginaAtual < questionario.dimensoes.length ? 'Continuar' : 'Finalizar') }}
        </button>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Adicione algum CSS básico para formatação */
.responder-container {
  max-width: 800px;
  margin: 2rem auto;
  padding: 2rem;
  border: 1px solid #ccc;
  border-radius: 8px;
}
.loading, .error-message {
  text-align: center;
  padding: 2rem;
}
.error-message {
  color: red;
}
.progresso {
  text-align: right;
  color: #666;
  margin-bottom: 1rem;
}
.pergunta-item {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px dashed #eee;
}
.opcoes-likert label {
  margin-right: 1rem;
  cursor: pointer;
}
.opcoes-likert input {
  margin-right: 0.3rem;
}
.consentimento {
  margin-top: 1rem;
}
.navegacao {
  margin-top: 2rem;
  display: flex;
  justify-content: space-between;
}
button {
  padding: 0.8rem 1.5rem;
  cursor: pointer;
}
button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style>