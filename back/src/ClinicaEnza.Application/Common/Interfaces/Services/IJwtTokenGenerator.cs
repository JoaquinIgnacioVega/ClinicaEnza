using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
    string GenerarToken(Usuario usuario) => GenerateToken(usuario);
}

