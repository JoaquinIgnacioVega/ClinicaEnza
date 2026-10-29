using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;

public class ObtenerHorariosPorMedicoQueryHandler : IRequestHandler<ObtenerHorariosPorMedicoQuery, Result<IReadOnlyList<HorarioAtencionDto>>>
{
    private readonly IHorarioAtencionRepository _horarioRepository;
    private readonly IRepository<Medico> _medicoRepository;

    public ObtenerHorariosPorMedicoQueryHandler(
        IHorarioAtencionRepository horarioRepository,
        IRepository<Medico> medicoRepository)
    {
        _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<IReadOnlyList<HorarioAtencionDto>>> Handle(
        ObtenerHorariosPorMedicoQuery request,
        CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<IReadOnlyList<HorarioAtencionDto>>($"No se encontró el médico con ID {request.IdMedico}.");

        var horarios = await _horarioRepository.ObtenerPorMedicoAsync(request.IdMedico, cancellationToken);

        var dtos = horarios.Select(h => new HorarioAtencionDto
        {
            IdHorarioAtencion = h.IdHorarioAtencion,
            IdMedico = h.IdMedico,
            NombreMedico = h.Medico?.ObtenerNombreCompleto() ?? medico.ObtenerNombreCompleto(),
            DiaSemana = h.DiaSemana,
            HoraDesde = h.HoraDesde,
            HoraHasta = h.HoraHasta,
            IdConsultorio = h.IdConsultorio,
            NumeroConsultorio = h.Consultorio?.Numero ?? string.Empty
        }).ToList();

        return Result.Success<IReadOnlyList<HorarioAtencionDto>>(dtos);
    }
}
