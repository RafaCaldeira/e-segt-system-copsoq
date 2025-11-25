import { createRouter, createWebHistory } from "vue-router";

import Login from '../pages/Login.vue';
import Cadastro from '../pages/Cadastro.vue';
import Dashboard from '../pages/Dashboard.vue';
import Relatorio from '../pages/Relatorio.vue';
import PlanoDeAcao from '../pages/PlanoDeAcao.vue';
import ResponderQuestionario from '../pages/ResponderQuestionario.vue';
import CriarQuestionario from '../pages/CriarQuestionario.vue';
import Funcionario from '../pages/Funcionario.vue';
import NovoFuncionario from "../pages/NovoFuncionario.vue";
import EditarFuncionario from "../pages/EditarFuncionario.vue";


const routes = [
  { path: '/login', name: 'Login', component: Login },
  { path: '/cadastro', name: 'cadastro', component: Cadastro },
  { path: '/dashboard', name: 'dashboard', component: Dashboard },
  { path: '/relatorio', name: 'relatorio', component: Relatorio },
  { path: '/plano-de-acao', name: 'PlanoDeAcao', component: PlanoDeAcao },
  { path: '/funcionario', name: 'Funcionarios', component: Funcionario },
  { path: '/novo-funcionario', name: 'NovoFuncionario', component: NovoFuncionario },
  { path: '/editarFuncionario/:id',name: 'EditarFuncionario',component: EditarFuncionario,props: true},

  // 👉 Nova rota adicionada
  { path: '/criar-questionario', name: 'CriarQuestionario', component: CriarQuestionario },

  { 
    path: '/responder/:token', 
    name: 'ResponderQuestionario',
    component: ResponderQuestionario,
    props: true 
  },

  { path: '/', redirect: '/login' },
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
