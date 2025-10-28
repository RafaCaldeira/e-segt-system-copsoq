using system_copsoq_api.Models;

namespace system_copsoq_api.Services
{
    public interface ITokenService
    {
        // Define um método que recebe um usuário e retorna o token (string)
        string CreateToken(User user);
    }
}