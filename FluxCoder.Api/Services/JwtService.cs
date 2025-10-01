using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluxCoder.Api.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FluxCoder.Api.Services;

public class JwtService(IOptionsMonitor<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.CurrentValue;

    public string GenerateJwtToken(string username, string role, int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _jwtSettings.Key ?? throw new InvalidOperationException("JWT ключ не найден")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        List<Claim> claims = 
        [
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ];
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}