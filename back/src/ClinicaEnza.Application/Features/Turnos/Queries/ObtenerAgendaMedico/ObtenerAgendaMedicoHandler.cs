using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerAgendaMedico;

public class ObtenerAgendaMedicoHandler : IRequestHandler<ObtenerAgendaMedicoQuery, Result<IEnumerable<TurnoAgendaMedicoDto>>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IRepository<Medico> _medicoRepository;

    public ObtenerAgendaMedicoHandler(
        ITurnoRepository turnoRepository,
        IRepository<Medico> medicoRepository)
    {
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<IEnumerable<TurnoAgendaMedicoDto>>> Handle(
        ObtenerAgendaMedicoQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.IdMedico != request.IdMedicoSolicitante)
        {
            return Result<IEnumerable<TurnoAgendaMedicoDto>>.Failure(
                "No tienes autorización para consultar la agenda de otro profesional.");
        }

        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
        {
            return Result<IEnumerable<TurnoAgendaMedicoDto>>.Failure("Médico no encontrado.");
        }

        var turnos = await _turnoRepository.ObtenerAgendaMedicoAsync(
            request.IdMedico,
            request.Fecha,
            request.Estado,
            cancellationToken);

        var dtos = turnos
            .OrderBy(t => t.HoraInicio)
            .Select(t => new TurnoAgendaMedicoDto
            {
                IdTurno = t.IdTurno,
                HoraInicio = t.HoraInicio,
                HoraFin = t.HoraFin,
                Estado = t.Estado,
                IdPaciente = t.IdPaciente,
                NombreCompletoPaciente = t.Paciente?.ObtenerNombreCompleto() ?? string.Empty,
                DniPaciente = t.Paciente?.Dni ?? string.Empty,
                ObraSocialPaciente = t.Paciente?.ObraSocial ?? string.Empty,
                EdadPaciente = t.Paciente?.CalcularEdad() ?? 0,
                IdConsultorio = t.IdConsultorio,
                UbicacionConsultorio = t.Consultorio?.ObtenerUbicacion() ?? string.Empty
            })
            .ToList();

        return Result<IEnumerable<TurnoAgendaMedicoDto>>.Success(dtos);
    }
}

