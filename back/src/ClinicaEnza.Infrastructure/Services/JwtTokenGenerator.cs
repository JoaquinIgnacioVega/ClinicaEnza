using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClinicaEnza.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string GenerateToken(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        var secret = _configuration["Jwt:Secret"] 
                     ?? _configuration["Jwt:Key"] 
                     ?? "ClinicaEnzaSuperSecretKeyForJwtSigning2026!";

        var issuer = _configuration["Jwt:Issuer"] ?? "ClinicaEnza";
        var audience = _configuration["Jwt:Audience"] ?? "ClinicaEnzaApp";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out var exp) ? exp : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdPersona.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.IdPersona.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Mail),
            new(ClaimTypes.Email, usuario.Mail),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("role", usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("legajo", usuario.Legajo),
            new("nombreCompleto", usuario.ObtenerNombreCompleto())
        };

        if (usuario.IdMedico.HasValue)
        {
            claims.Add(new Claim("idMedico", usuario.IdMedico.Value.ToString()));
            claims.Add(new Claim("IdMedico", usuario.IdMedico.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}

