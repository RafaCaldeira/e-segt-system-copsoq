using System.Threading.Tasks;

namespace system_copsoq_api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailDestino, string assunto, string mensagemHtml);
    }
}