using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication2.Models;

namespace WebApplication2.Helpers
{
    public static class JwtTokenGenerator
    {
        public static string GenerateToken(ApplicationUser user, IList<string> roles, IConfiguration config)
        {
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtConfig:Key"]));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtConfig:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: config["JwtConfig:Issuer"],
                audience: config["JwtConfig:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["JwtConfig:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
