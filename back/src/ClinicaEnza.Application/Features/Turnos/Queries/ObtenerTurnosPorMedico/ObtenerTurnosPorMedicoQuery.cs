using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;

public record ObtenerTurnosPorMedicoQuery(
    int IdMedico,
    DateTime? Fecha = null,
    EstadoTurno? Estado = null) : IRequest<Result<IReadOnlyList<TurnoDto>>>;

