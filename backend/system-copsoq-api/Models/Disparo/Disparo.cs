using System;
using System.ComponentModel.DataAnnotations;
using system_copsoq_api.Models.Formularios;
using System.Collections.Generic;
using system_copsoq_api.Models;

namespace system_copsoq_api.Models.Disparo
{
    public class Disparo
    {
        public int ID { get; set; }

        [Required]
        public int QuestionarioID { get; set; }
        public Questionario Questionario { get; set; } = null!; // Vem de ...Models.Formularios

        [Required]
        public int FuncionarioID { get; set; }
        public Funcionario Funcionario { get; set; } = null!; // Vem de ...Models

        [Required]
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid TokenAcesso { get; set; } = Guid.NewGuid();

        public DateTime? DataResposta { get; set; }
        public bool Respondido { get; set; } = false;

        // 'RespostaFuncionario' é encontrado porque está no MESMO namespace
        public ICollection<RespostaFuncionario> Respostas { get; set; } = new List<RespostaFuncionario>();
    }
}