using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;

public record ObtenerTurnosPorMedicoQuery(int IdMedico) : IRequest<Result<IReadOnlyList<TurnoDto>>>;

