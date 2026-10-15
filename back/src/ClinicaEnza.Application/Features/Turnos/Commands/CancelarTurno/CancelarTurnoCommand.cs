using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurno;

public record CancelarTurnoCommand(int IdTurno) : IRequest<Result>;

