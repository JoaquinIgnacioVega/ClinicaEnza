using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.ModificarUsuario;

public class ModificarUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public int? IdMedico { get; set; }

    public ModificarUsuarioCommand() { }

    public ModificarUsuarioCommand(
        int idUsuario,
        string nombre,
        string apellido,
        string mail,
        RolUsuario rol,
        int? idMedico = null)
    {
        IdUsuario = idUsuario;
        Nombre = nombre;
        Apellido = apellido;
        Mail = mail;
        Rol = rol;
        IdMedico = idMedico;
    }
}

