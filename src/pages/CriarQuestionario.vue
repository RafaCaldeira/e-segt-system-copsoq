<script setup lang="ts">
import { ref } from 'vue';
import { apiService } from '../services/api.service';
import type { QuestionarioCreateDto, OpcaoRespostaCreateDto, DimensaoCreateDto, PerguntaCreateDto } from '../types/questionario.types';
import { useRouter } from 'vue-router';

const router = useRouter();
const step = ref(1); // 1=Info, 2=Escala, 3=Estrutura (Dimensões/Perguntas)
const isLoading = ref(false);

// Dados do Questionário
const novoQuestionarioId = ref<number | null>(null);
const questionarioData = ref<QuestionarioCreateDto>({
  titulo: '',
  descricao: '',
  textoIntroducao: '',
  textoConsentimento: '',
  setoresAplicaveis: [] // (Pode adicionar checkboxes para selecionar setores depois)
});

// Dados da Escala
const escalaOpcoes = ref<OpcaoRespostaCreateDto[]>([
  { texto: 'Discordo totalmente', valor: 1, ordem: 1 },
  { texto: 'Discordo', valor: 2, ordem: 2 },
  { texto: 'Neutro', valor: 3, ordem: 3 },
  { texto: 'Concordo', valor: 4, ordem: 4 },
  { texto: 'Concordo totalmente', valor: 5, ordem: 5 }
]);

// Dados da Estrutura (Dimensões e Perguntas)
// Uma estrutura local para construir antes de enviar
interface DimensaoLocal extends DimensaoCreateDto {
  tempId: number; // ID temporário para UI
  perguntas: PerguntaCreateDto[];
}
const dimensoes = ref<DimensaoLocal[]>([]);

// --- Funções ---

async function criarQuestionarioBase() {
  isLoading.value = true;
  const result = await apiService.createQuestionario(questionarioData.value);
  if (result) {
    novoQuestionarioId.value = result.id;
    step.value = 2; // Avança para Escala
  } else {
    alert('Erro ao criar questionário.');
  }
  isLoading.value = false;
}

async function salvarEscala() {
  if (!novoQuestionarioId.value) return;
  isLoading.value = true;
  
  for (const opcao of escalaOpcoes.value) {
    await apiService.createOpcaoResposta(novoQuestionarioId.value, opcao);
  }
  
  step.value = 3; // Avança para Dimensões
  isLoading.value = false;
}

function addDimensao() {
  dimensoes.value.push({
    titulo: '',
    nomeIndicador: '',
    ordem: dimensoes.value.length + 1,
    tempId: Date.now(),
    perguntas: []
  });
}

function addPergunta(dimensaoIndex: number) {
  const dim = dimensoes.value[dimensaoIndex];
  if (!dim) return;
  dim.perguntas.push({ texto: '' });
}

async function finalizarCadastro() {
  if (!novoQuestionarioId.value) return;
  isLoading.value = true;

  for (const dim of dimensoes.value) {
    // 1. Criar Dimensão
    const dimCriada = await apiService.createDimensao(novoQuestionarioId.value, {
      titulo: dim.titulo,
      nomeIndicador: dim.nomeIndicador,
      ordem: dim.ordem
    });

    if (dimCriada) {
      // 2. Criar Perguntas dessa Dimensão
      for (const perg of dim.perguntas) {
        await apiService.createPergunta(novoQuestionarioId.value, dimCriada.id, {
          texto: perg.texto
        });
      }
    }
  }

  alert('Questionário criado com sucesso!');
  router.push('/dashboard'); // Volta ao dashboard (Admin)
  isLoading.value = false;
}
</script>

<template>
  <div class="app-layout">
    <!-- (Adicione a Sidebar aqui, igual às outras páginas) -->
    
    <main class="main-content">
      <div class="responder-container">
        <h1 class="content-title">Criar Novo Formulário</h1>

        <!-- PASSO 1: Informações Básicas -->
        <div v-if="step === 1">
          <h2>Passo 1: Informações Básicas</h2>
          <div class="form-group">
            <label>Título</label>
            <input v-model="questionarioData.titulo" placeholder="Ex: Avaliação de Burnout">
          </div>
          <div class="form-group">
            <label>Descrição</label>
            <textarea v-model="questionarioData.descricao"></textarea>
          </div>
          <div class="form-group">
            <label>Texto de Introdução</label>
            <textarea v-model="questionarioData.textoIntroducao"></textarea>
          </div>
          <div class="form-group">
            <label>Termo de Consentimento</label>
            <textarea v-model="questionarioData.textoConsentimento"></textarea>
          </div>
          <button class="btn-continuar" @click="criarQuestionarioBase" :disabled="isLoading">
            {{ isLoading ? 'Criando...' : 'Próximo: Definir Escala' }}
          </button>
        </div>

        <!-- PASSO 2: Escala de Resposta -->
        <div v-if="step === 2">
          <h2>Passo 2: Escala de Resposta</h2>
          <p>Defina as opções que o funcionário poderá escolher.</p>
          
          <div v-for="(opcao, index) in escalaOpcoes" :key="index" class="opcao-item">
             <span>Valor: {{ opcao.valor }}</span>
             <input v-model="opcao.texto" placeholder="Texto da opção">
          </div>
          <button @click="escalaOpcoes.push({ texto: '', valor: escalaOpcoes.length + 1, ordem: escalaOpcoes.length + 1 })">
            + Adicionar Opção
          </button>
          
          <br><br>
          <button class="btn-continuar" @click="salvarEscala" :disabled="isLoading">
            {{ isLoading ? 'Salvando...' : 'Próximo: Adicionar Perguntas' }}
          </button>
        </div>

        <!-- PASSO 3: Estrutura (Dimensões e Perguntas) -->
        <div v-if="step === 3">
          <h2>Passo 3: Perguntas</h2>
          
          <div v-for="(dim, index) in dimensoes" :key="dim.tempId" class="dimensao-box">
            <h3>Tópico / Dimensão {{ index + 1 }}</h3>
            <input v-model="dim.titulo" placeholder="Título da Página (ex: Exaustão)">
            <input v-model="dim.nomeIndicador" placeholder="Nome do Indicador (para Relatório)">
            
            <div class="perguntas-lista">
              <h4>Perguntas</h4>
              <div v-for="(perg, pIndex) in dim.perguntas" :key="pIndex">
                <input v-model="perg.texto" placeholder="Texto da Pergunta" style="width: 80%">
              </div>
              <button @click="addPergunta(index)">+ Adicionar Pergunta</button>
            </div>
          </div>

          <button class="btn-add-dimensao" @click="addDimensao">+ Adicionar Novo Tópico</button>
          
          <br><br><br>
          <button class="btn-continuar" @click="finalizarCadastro" :disabled="isLoading">
            {{ isLoading ? 'Finalizando...' : 'Salvar Formulário Completo' }}
          </button>
        </div>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* Adicione estilos para form-group, inputs, botões, etc. */
/* ... (Reutilize os estilos que já tem) ... */
/* CONTAINER PRINCIPAL */
.responder-container {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
}

/* TÍTULOS */
.content-title {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 2rem;
  text-align: center;
  color: #ffffff;
}

h2 {
  margin-bottom: 1.2rem;
  color: #444;
  font-size: 1.4rem;
  border-left: 5px solid #FA9021;
  padding-left: 10px;
}

h3 {
  margin-top: 1rem;
  font-size: 1.2rem;
  color: #555;
}

h4 {
  color: #666;
  margin-bottom: 0.5rem;
}

/* GRUPO DE FORM */
.form-group {
  margin-bottom: 1.4rem;
}

label {
  font-weight: 600;
  margin-bottom: 0.4rem;
  display: block;
  color: #ffffff;
}

/* INPUTS */
input,
textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d6d6d6;
  background: #fafafa;
  border-radius: 6px;
  font-size: 0.95rem;
  transition: all 0.2s;
}

input:focus,
textarea:focus {
  border-color: #FA9021;
  background: #fff;
  outline: none;
  box-shadow: 0 0 6px rgba(250, 144, 33, 0.3);
}

/* CARD DE DIMENSÕES */
.dimensao-box {
  border: 1px solid #e5e5e5;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  border-radius: 12px;
  background-color: #ffffff;
  box-shadow: 0px 3px 10px rgba(0,0,0,0.05);
}

/* LISTA DE PERGUNTAS */
.perguntas-lista {
  margin-top: 1rem;
  padding-left: 1rem;
  border-left: 3px solid #FA9021;
}

/* ITENS DE OPÇÃO */
.opcao-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.7rem 1rem;
  background: #fff;
  border: 1px solid #e6e6e6;
  border-radius: 8px;
  margin-bottom: 1rem;
  box-shadow: 0px 2px 5px rgba(0,0,0,0.05);
}

.opcao-item input {
  flex: 1;
}

/* BOTÕES GERAIS */
button {
  cursor: pointer;
  border: none;
  background: #FA9021;
  color: white;
  padding: 0.7rem 1.4rem;
  font-size: 1rem;
  border-radius: 6px;
  transition: 0.2s;
  font-weight: 600;
}

button:hover:not([disabled]) {
  background: #e47f18;
}

button:disabled {
  opacity: 0.5;
}

/* BOTÃO SECUNDÁRIO */
.btn-add-dimensao {
  margin-top: 1rem;
  background: #444;
}

.btn-add-dimensao:hover {
  background: #222;
}

.btn-continuar {
  margin-top: 2rem;
  width: 100%;
  padding: 1rem;
  font-size: 1.1rem;
  border-radius: 10px;
}

</style>