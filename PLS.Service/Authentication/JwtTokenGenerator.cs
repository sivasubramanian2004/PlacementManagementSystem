using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PMS.Service.Authentication
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public (string token, DateTime expiresAt) GenerateToken(int userId, string email, string role, string firstName, string lastName)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            /*
            NameIdentifier → User ID
            Email          → Email
            Role           → User Role
            GivenName      → First Name
            Surname        → Last Name
            Jti            → Unique JWT ID
             */

            var claims = new List<Claim>
            {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(ClaimTypes. GivenName, firstName),
            new(ClaimTypes.Surname, lastName  ),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
