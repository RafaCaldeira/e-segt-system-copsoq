using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Importante para converter o Enum em texto no JSON

namespace system_copsoq_api.Models.Planos
{
    // 1. Definição do Enum (Pendente, Concluido, EmAndamento)
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusAcao
    {
        Pendente,
        Concluido,
        EmAndamento
    }

    public class Acao
    {
        public int ID { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;

        public DateTime? Prazo { get; set; }

        // 2. O Status agora usa o Enum definido acima
        [Required]
        public StatusAcao Status { get; set; } = StatusAcao.Pendente;

        // 3. NOVOS CAMPOS (Para o modal de justificativa)
        public DateTime? DataConclusao { get; set; } // Pode ser nulo
        
        public string? Justificativa { get; set; }   // Pode ser nulo

        // --- Relação com o Plano ---
        [Required]
        public int PlanoDeAcaoID { get; set; }
        
        [JsonIgnore] // Evita erro de "ciclo infinito" ao converter para JSON
        public PlanoDeAcao PlanoDeAcao { get; set; } = null!;
    }
}