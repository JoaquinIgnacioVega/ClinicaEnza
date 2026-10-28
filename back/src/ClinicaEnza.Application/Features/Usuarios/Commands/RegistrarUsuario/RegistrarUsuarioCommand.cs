using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string Legajo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public int? IdMedico { get; set; }

    public RegistrarUsuarioCommand() { }

    public RegistrarUsuarioCommand(
        string nombre,
        string apellido,
        string dni,
        DateTime fechaNacimiento,
        string mail,
        string legajo,
        string password,
        RolUsuario rol,
        int? idMedico = null)
    {
        Nombre = nombre;
        Apellido = apellido;
        Dni = dni;
        FechaNacimiento = fechaNacimiento;
        Mail = mail;
        Legajo = legajo;
        Password = password;
        Rol = rol;
        IdMedico = idMedico;
    }
}

