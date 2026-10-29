using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.DarDeBajaUsuario;

public record DarDeBajaUsuarioCommand(int IdUsuario) : IRequest<Result>
{
    public DarDeBajaUsuarioCommand() : this(0) { }
}

