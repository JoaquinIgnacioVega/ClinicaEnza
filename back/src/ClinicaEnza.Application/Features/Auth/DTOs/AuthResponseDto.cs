using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string RolNombre => Rol.ToString();
    public int? IdMedico { get; set; }

    public AuthResponseDto() { }

    public AuthResponseDto(string token, string nombreCompleto, RolUsuario rol, int? idMedico = null)
    {
        Token = token;
        NombreCompleto = nombreCompleto;
        Rol = rol;
        IdMedico = idMedico;
    }
}

