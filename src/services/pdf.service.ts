import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import type { RelatorioCompletoDto } from '../types/relatorio.types';

export const pdfService = {
  gerarRelatorioPDF(relatorio: RelatorioCompletoDto) {
    const doc = new jsPDF();

    // --- Cabeçalho ---
    doc.setFontSize(18);
    doc.text('Relatório de Avaliação COPSOQ', 14, 20);

    doc.setFontSize(12);
    doc.text(`Questionário: ${relatorio.tituloQuestionario}`, 14, 30);
    doc.text(`Data de Geração: ${new Date().toLocaleDateString()}`, 14, 36);
    doc.text(`Total de Respondentes: ${relatorio.totalRespondentes}`, 14, 42);

    // --- Tabela de Resultados ---
    // Prepara os dados para a tabela
    const tableData = relatorio.resultados.map(item => [
      item.nomeIndicador,
      `${item.scorePercentual.toFixed(1)}%`, // Formata a %
      item.nivelRisco
    ]);

    autoTable(doc, {
      startY: 50,
      head: [['Indicador', 'Resultado', 'Nível de Risco']],
      body: tableData,
      theme: 'striped',
      headStyles: { fillColor: [59, 130, 246] }, // Azul (cor do seu tema)
      // Lógica para colorir o texto do risco (opcional)
      didParseCell: (data) => {
        if (data.section === 'body' && data.column.index === 2) {
          const risco = data.cell.raw as string;
          if (risco.includes('Alto') || risco.includes('Baixo apoio')) {
            data.cell.styles.textColor = [217, 83, 79]; // Vermelho
          } else if (risco.includes('Baixo') || risco.includes('saudável')) {
             data.cell.styles.textColor = [92, 184, 92]; // Verde
          }
        }
      }
    });

    // --- Rodapé / Notas ---
    const finalY = (doc as any).lastAutoTable.finalY || 50;
    doc.setFontSize(10);
    doc.text('Este relatório foi gerado automaticamente pelo sistema E-SegT.', 14, finalY + 10);

    // --- Guardar o Ficheiro ---
    doc.save(`Relatorio_COPSOQ_${new Date().toISOString().slice(0,10)}.pdf`);
  }
};