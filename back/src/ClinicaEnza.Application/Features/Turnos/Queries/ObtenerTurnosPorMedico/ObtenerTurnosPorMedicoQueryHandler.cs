using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;

public class ObtenerTurnosPorMedicoQueryHandler : IRequestHandler<ObtenerTurnosPorMedicoQuery, Result<IReadOnlyList<TurnoDto>>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IRepository<ClinicaEnza.Domain.Medico> _medicoRepository;

    public ObtenerTurnosPorMedicoQueryHandler(
        ITurnoRepository turnoRepository,
        IRepository<ClinicaEnza.Domain.Medico> medicoRepository)
    {
        _turnoRepository = turnoRepository;
        _medicoRepository = medicoRepository;
    }

    public async Task<Result<IReadOnlyList<TurnoDto>>> Handle(ObtenerTurnosPorMedicoQuery request, CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<IReadOnlyList<TurnoDto>>($"No se encontró el médico con ID {request.IdMedico}.");

        var turnos = await _turnoRepository.ObtenerPorMedicoAsync(
            request.IdMedico, request.Fecha, request.Estado, cancellationToken);

        var dtos = turnos.Select(t => new TurnoDto
        {
            IdTurno = t.IdTurno,
            HoraInicio = t.HoraInicio,
            HoraFin = t.HoraFin,
            Estado = t.Estado,
            IdPaciente = t.IdPaciente,
            NombrePaciente = t.Paciente?.ObtenerNombreCompleto(),
            DniPaciente = t.Paciente?.Dni,
            ObraSocialPaciente = t.Paciente?.ObraSocial,
            EdadPaciente = t.Paciente?.CalcularEdad(),
            IdMedico = t.IdMedico,
            NombreMedico = t.Medico?.ObtenerNombreCompleto(),
            IdConsultorio = t.IdConsultorio,
            NumeroConsultorio = t.Consultorio?.Numero,
            UbicacionConsultorio = t.Consultorio?.ObtenerUbicacion()
        }).ToList();

        return Result.Success<IReadOnlyList<TurnoDto>>(dtos);
    }
}

