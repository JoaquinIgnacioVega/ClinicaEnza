using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;

public record ObtenerHorariosPorMedicoQuery(int IdMedico)
    : IRequest<Result<IReadOnlyList<HorarioAtencionDto>>>;
