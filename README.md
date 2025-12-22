# System COPSOQ – E-SegT Frontend

Painel administrativo e de gestão para **avaliação de riscos psicossociais (COPSOQ)**, desenvolvido para a **E-SegT**.
O sistema permite o gerenciamento de **empresas, funcionários, questionários e planos de ação**, adaptando a interface conforme o perfil de acesso: **Administrador, Psicólogo ou Cliente**.

---

## 🚀 Tecnologias Utilizadas

* **Vue.js 3** (Composition API + Script Setup)
* **TypeScript**
* **Vite**
* **Pinia** (Gerenciamento de Estado)
* **Vue Router**
* **CSS3** (Layout responsivo e customizado)

---

## ⚙️ Funcionalidades Principais

### 👤 Perfis de Acesso

* **Administrador:** Acesso total ao sistema (gestão de empresas, criação e envio de questionários).
* **Psicólogo:** Acesso voltado à análise (relatórios, planos de ação e histórico).
* **Cliente:** Acesso restrito à gestão de seus próprios colaboradores.

### 📦 Módulos do Sistema

* **Dashboard:** Visão geral do sistema e seleção de empresas.
* **Funcionários:** Cadastro manual, edição e **importação via CSV**.
* **Questionários:** Criação e disparo de formulários de avaliação COPSOQ.
* **Relatórios:** Visualização gráfica de resultados e indicadores de risco psicossocial.
* **Plano de Ação:** Gerenciamento de melhorias com **geração de PDF**.
* **Histórico:** Monitoramento de envios e respostas aos questionários.

---

## ⚙️ Como Rodar o Projeto

O projeto é dividido em **Backend (.NET)** e **Frontend (Vue 3)**.
Siga os passos abaixo para executar o sistema localmente.

---

## 🖥️ Backend (.NET)

### 1️⃣ Clonar o repositório

```bash
git clone https://github.com/RafaCaldeira/youtube-challenge.git
cd youtube-challenge
```

### 2️⃣ Instalar o .NET SDK (caso não tenha)

```bash
winget install Microsoft.DotNet.SDK.9
```

💡 Verifique a instalação:

```bash
dotnet --version
```

### 3️⃣ Entrar na pasta do backend

```bash
cd backend
```

### 4️⃣ Executar a API

```bash
dotnet run
```

O backend ficará disponível, por padrão, em:

```
https://localhost:5001
ou
http://localhost:5000
```

---

## 🌐 Frontend (Vue 3)

### 1️⃣ Entrar na pasta do frontend

```bash
cd frontend
```

### 2️⃣ Instalar as dependências

```bash
npm install
```

### 3️⃣ Iniciar a aplicação

```bash
npm run dev
```

A aplicação frontend estará disponível em:

```
http://localhost:5173
```

⚠️ **Importante:**
Certifique-se de que o **backend esteja rodando antes de iniciar o frontend**, pois o frontend consome a API REST do backend.

---

## 📂 Estrutura de Pastas (Resumo)

```text
src/
├── components/   # Componentes reutilizáveis (ex: AppFooter.vue)
├── pages/        # Telas principais do sistema
├── store/        # Gerenciamento de estado (auth, usuário, etc.)
├── services/     # Comunicação com a API (Backend em C#)
├── types/        # Tipagens TypeScript
```

---

## 📝 Próximos Passos (Roadmap)

* [ ] Verificar se a integração com o **Backend C#** está recebendo corretamente o arquivo **CSV** na rota de importação.
* [ ] Testar o fluxo completo de **"Esqueci minha senha"** (caso implementado).
* [ ] Refinar os **gráficos do Dashboard** (cores, responsividade e interatividade).

---

## 👨‍💻 Autor

Desenvolvido por **Rafael Caldeira**.

---

## 📤 Enviando alterações para o GitHub

```bash
git add README.md
git commit -m "docs: update README with project overview and setup instructions"
git push origin main
```
