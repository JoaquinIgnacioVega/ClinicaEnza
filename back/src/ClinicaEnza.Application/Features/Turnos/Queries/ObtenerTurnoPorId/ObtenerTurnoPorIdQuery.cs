using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnoPorId;

public record ObtenerTurnoPorIdQuery(int IdTurno) : IRequest<Result<TurnoDto>>;

