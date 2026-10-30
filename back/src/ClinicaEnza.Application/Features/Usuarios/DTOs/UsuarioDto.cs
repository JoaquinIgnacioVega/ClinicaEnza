using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.DTOs;

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public int IdPersona { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Apellido.ToUpper()}, {Nombre}";
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string Legajo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public RolUsuario Rol { get; set; }
    public string RolNombre => Rol.ToString();
    public bool Activo { get; set; }
    public int? IdMedico { get; set; }
}

