using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string token, DateTime expiresAt) GenerateToken(User user, bool rememberMe)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = rememberMe
                ? DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RememberMeExpiryDays"]!))
                : DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpiryMinutes"]!));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.EmployeeId.HasValue)
            {
                claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }

}
