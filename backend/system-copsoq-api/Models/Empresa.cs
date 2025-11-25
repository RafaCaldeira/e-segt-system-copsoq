using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using system_copsoq_api.Models.Planos; 
using system_copsoq_api.Models.Disparo;
namespace system_copsoq_api.Models
{
    public class Empresa
    {
        public int ID { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string NomeResponsavel { get; set; } = string.Empty;
        public SetorAtuacao SetorAtuacao { get; set; }
        public String Cidade { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public bool IsAtivo { get; set; } = true;
        public ICollection<User> Usuarios { get; set; } = new List<User>();
        public ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();

        public ICollection<PlanoDeAcao> PlanosDeAcao { get; set; } = new List<PlanoDeAcao>();
    }
}