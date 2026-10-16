using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;

public class ObtenerHorariosPorMedicoQueryHandler : IRequestHandler<ObtenerHorariosPorMedicoQuery, Result<IReadOnlyList<HorarioAtencionDto>>>
{
    private readonly IHorarioAtencionRepository _horarioRepository;

    public ObtenerHorariosPorMedicoQueryHandler(IHorarioAtencionRepository horarioRepository)
    {
        _horarioRepository = horarioRepository;
    }

    public async Task<Result<IReadOnlyList<HorarioAtencionDto>>> Handle(
        ObtenerHorariosPorMedicoQuery request,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _horarioRepository.ObtenerPorMedicoAsync(request.IdMedico, cancellationToken);

        var dtos = horarios.Select(h => new HorarioAtencionDto
        {
            IdHorarioAtencion = h.IdHorarioAtencion,
            IdMedico = h.IdMedico,
            NombreMedico = h.Medico.ObtenerNombreCompleto(),
            DiaSemana = h.DiaSemana,
            HoraDesde = h.HoraDesde,
            HoraHasta = h.HoraHasta,
            IdConsultorio = h.IdConsultorio,
            NumeroConsultorio = h.Consultorio.Numero
        }).ToList();

        return Result.Success<IReadOnlyList<HorarioAtencionDto>>(dtos);
    }
}
