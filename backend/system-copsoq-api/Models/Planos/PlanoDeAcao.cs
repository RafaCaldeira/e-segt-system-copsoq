using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace system_copsoq_api.Models.Planos
{
    public class PlanoDeAcao
    {
        public int ID { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty; 

        public string Descricao { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public bool IsAtivo { get; set; } = true;

        // --- Relação com a Empresa ---
        [Required]
        public int EmpresaID { get; set; }
        public Empresa Empresa { get; set; } = null!;

        // --- Relação com as Tarefas ---
        public ICollection<Acao> Acoes { get; set; } = new List<Acao>();
    }
}