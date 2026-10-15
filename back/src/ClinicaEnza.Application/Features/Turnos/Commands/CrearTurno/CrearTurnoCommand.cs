using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;

public record CrearTurnoCommand(
    int IdPaciente,
    int IdMedico,
    int IdConsultorio,
    DateTime HoraInicio
) : IRequest<Result<TurnoDto>>;

