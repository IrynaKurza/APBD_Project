using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Project.Models;
using Microsoft.IdentityModel.Tokens;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;   
    }
    
    public string CreateAccessToken(Employee employee, string roleName)
    {
        var claims = new []
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Name, employee.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SymmetricSecurityKey"]!));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"],
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = signingCredentials,
            Expires = DateTime.Now.AddHours(1)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token); 
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[96];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}