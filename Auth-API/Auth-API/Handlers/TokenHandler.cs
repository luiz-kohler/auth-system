using Auth_API.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_API.Handlers
{
    public class TokenHandler : ITokenHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptHandler _encryptHandler;

        public TokenHandler(IConfiguration configuration, IEncryptHandler encryptHandler)
        {
            _configuration = configuration;
            _encryptHandler = encryptHandler;
        }

        public string Generate(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier,  _encryptHandler.Encrypt(user.Id.ToString())),
                }),
                Expires = DateTime.UtcNow.AddDays(999),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Project"],
                IssuedAt = DateTime.UtcNow
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public int ExtractUserId(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Authorization token is missing.");

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                throw new UnauthorizedAccessException("Invalid authorization token.");

            var jwtToken = handler.ReadJwtToken(token);
            var userIdEcrypted = jwtToken.Claims.FirstOrDefault(c => c.Type == TokenClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdEcrypted))
                throw new UnauthorizedAccessException("User identifier claim is missing in the token.");

            var userIdDecrypted = _encryptHandler.Decrypt(userIdEcrypted);

            return int.Parse(userIdDecrypted);
        }
    }

    public interface ITokenHandler
    {
        string Generate(User user);
        int ExtractUserId(string token);

    }

    public static class TokenClaimTypes
    {
        public static string NameIdentifier => "nameid";
    }
}
