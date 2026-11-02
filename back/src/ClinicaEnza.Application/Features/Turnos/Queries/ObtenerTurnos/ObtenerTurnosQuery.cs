using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnos;

public record ObtenerTurnosQuery(
    DateTime? Fecha,
    int? IdPaciente,
    int? IdMedico,
    EstadoTurno? Estado) : IRequest<Result<IReadOnlyList<TurnoDto>>>;
