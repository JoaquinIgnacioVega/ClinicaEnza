using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerAgendaMedico;

public record ObtenerAgendaMedicoQuery(
    int IdMedico,
    int IdMedicoSolicitante,
    DateTime? Fecha = null,
    EstadoTurno? Estado = null) : IRequest<Result<IEnumerable<TurnoAgendaMedicoDto>>>;

