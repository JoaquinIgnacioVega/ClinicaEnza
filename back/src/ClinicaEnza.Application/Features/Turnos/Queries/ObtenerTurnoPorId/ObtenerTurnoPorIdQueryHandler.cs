using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnoPorId;

public class ObtenerTurnoPorIdQueryHandler : IRequestHandler<ObtenerTurnoPorIdQuery, Result<TurnoDto>>
{
    private readonly ITurnoRepository _turnoRepository;

    public ObtenerTurnoPorIdQueryHandler(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    public async Task<Result<TurnoDto>> Handle(ObtenerTurnoPorIdQuery request, CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.ObtenerDetallePorIdAsync(request.IdTurno, cancellationToken);
        if (turno == null)
            return Result.Failure<TurnoDto>($"No se encontró el turno con ID {request.IdTurno}.");

        var dto = new TurnoDto
        {
            IdTurno = turno.IdTurno,
            HoraInicio = turno.HoraInicio,
            HoraFin = turno.HoraFin,
            Estado = turno.Estado,
            IdPaciente = turno.IdPaciente,
            NombrePaciente = turno.Paciente != null ? turno.Paciente.ObtenerNombreCompleto() : null,
            IdMedico = turno.IdMedico,
            NombreMedico = turno.Medico != null ? turno.Medico.ObtenerNombreCompleto() : null,
            IdConsultorio = turno.IdConsultorio,
            NumeroConsultorio = turno.Consultorio?.Numero
        };

        return Result.Success(dto);
    }
}

