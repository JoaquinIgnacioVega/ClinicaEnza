using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurnoPaciente;

public record CancelarTurnoPacienteCommand(int IdTurno, int IdPaciente) : IRequest<Result<TurnoDto>>;

