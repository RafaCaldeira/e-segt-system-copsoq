<script setup lang="ts">
// Nenhuma lógica complexa necessária, apenas exibição
</script>

<template>
  <footer class="app-footer">
    <div class="footer-content">
      <div class="footer-section">
        <h4>eSegT Cons. e Asses. em Seg. do Trabalho</h4>
        <p>Email: contato@esegt.com.br</p>
        <p>Telefone: (55) 3414-1698</p>
        <p>Endereço: R. 15 de Novembro, 3710 - Centro, Uruguaiana - RS, 97502-832</p>
      </div>
      <div class="footer-section">
        <h4>Psicóloga Responsável</h4>
        <p>Dra. Ana Paula Claus Bastos (CRP 07/16033)</p>
        <p>Email: anapclausbastos08@gmail.com</p>
        <p>Endereço: R. Santana, 2855 Esq. Domingos de Almeida</p>
      </div>
    </div>
    <div class="footer-bottom">
      &copy; {{ new Date().getFullYear() }} E-SegT. Todos os direitos reservados.
    </div>
  </footer>
</template>

<style scoped>
/* --- FOOTER PRETO --- */
.app-footer {
  background-color: #1a1a1a;
  color: #ffffff;
  padding: 2rem;
  margin-top: auto; /* Garante que fique no fundo se o pai for flex column */
  width: 100%;
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-around;
  flex-wrap: wrap;
  gap: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid #333;
}

.footer-section h4 {
  color: #3b82f6;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.footer-section p {
  color: #cccccc;
  margin: 0.5rem 0;
  font-size: 0.9rem;
}

.footer-bottom {
  text-align: center;
  padding-top: 1.5rem;
  font-size: 0.85rem;
  color: #777;
}

/* Responsividade do Footer */
@media (max-width: 900px) {
  .footer-content {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }
}
</style>