using Microsoft.IdentityModel.Tokens;
using system_copsoq_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace system_copsoq_api.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Pega a chave secreta do appsettings.json
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:ChaveSecreta"]!));
        }

        public string CreateToken(User user)
        {
            // 1. Criar as "Claims" (informações que vão dentro do token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) 
            };

            // 2. Criar as credenciais de assinatura
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            // 3. Descrever o token (quem emite, para quem, o conteúdo, expiração)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), 
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            // 4. Criar e serializar o token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Retorna o token como string
        }
    }
}