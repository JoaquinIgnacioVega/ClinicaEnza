using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Commands.ReagendarTurno;

public record ReagendarTurnoCommand(int IdTurno, DateTime NuevaHoraInicio) : IRequest<Result<TurnoDto>>;
