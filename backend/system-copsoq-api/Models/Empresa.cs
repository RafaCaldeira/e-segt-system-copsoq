namespace system_copsoq_api.Models
{
    public class Empresa
    {
        public int ID { get; set; }
        public string NomeEmpresa {get; set;} = string.Empty;
        public string NomeResponsavel {get; set;} = string.Empty;
        public String SetorAtuacao {get; set;} = string.Empty;
        public String Cidade {get; set;} = string.Empty;
        public String Email {get; set;} = string.Empty;
        public String Cnpj {get; set;} = string.Empty;
        public String Senha {get; set;} = string.Empty;
    }
}