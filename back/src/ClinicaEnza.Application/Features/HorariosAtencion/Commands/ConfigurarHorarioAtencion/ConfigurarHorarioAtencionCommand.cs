using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;

public record ConfigurarHorarioAtencionCommand(
    int IdMedico,
    DayOfWeek DiaSemana,
    TimeSpan HoraDesde,
    TimeSpan HoraHasta,
    int IdConsultorio
) : IRequest<Result<HorarioAtencionDto>>;
