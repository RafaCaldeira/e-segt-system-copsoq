namespace system_copsoq_api.Models.Formularios
{
    // Tabela de ligação que "Etiqueta" um Questionário a um Setor
    public class QuestionarioSetorAplicavel
    {
        public int ID { get; set; }

        // Chaves Estrangeiras
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!;
        
        public SetorAtuacao Setor { get; set; } // O enum que já criada (Industria, Saude, etc.)
    }
}