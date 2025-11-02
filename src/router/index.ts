import { createRouter, createWebHistory } from "vue-router";
import Login from '../pages/Login.vue'
import Cadastro from '../pages/Cadastro.vue'
import Dashboard from '../pages/Dashboard.vue'
import Relatorio from '../pages/Relatorio.vue'
import ResponderQuestionario from '../pages/ResponderQuestionario.vue';

const routes = [
    {path: '/login', name: 'Login', component: Login},
    {path: '/cadastro', name: 'cadastro', component: Cadastro},
    {path: '/dashboard', name: 'dashboard', component: Dashboard},
    {path: '/relatorio', name: 'relatorio', component: Relatorio},
    {path: '/responder/:token', name: 'ResponderQuestionario', component: ResponderQuestionario,props: true, },
    {path: '/',redirect: '/login',},
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router