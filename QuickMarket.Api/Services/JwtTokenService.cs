using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QuickMarket.Api.Services
{
    public interface IJwtTokenService
    {
        string CreateToken(string username, string gmail, string subjectId, string rol);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _cfg;

        public JwtTokenService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public string CreateToken(string username, string email, string subjectId, string rol)
        {
            var keyStr = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");
            if (keyStr.Length < 32)
                throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, subjectId),
                new(JwtRegisteredClaimNames.UniqueName, username),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.Role, rol)
            };

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
