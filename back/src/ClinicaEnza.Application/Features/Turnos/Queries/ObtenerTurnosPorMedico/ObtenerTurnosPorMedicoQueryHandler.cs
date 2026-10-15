using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;

public class ObtenerTurnosPorMedicoQueryHandler : IRequestHandler<ObtenerTurnosPorMedicoQuery, Result<IReadOnlyList<TurnoDto>>>
{
    private readonly ITurnoRepository _turnoRepository;

    public ObtenerTurnosPorMedicoQueryHandler(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    public async Task<Result<IReadOnlyList<TurnoDto>>> Handle(ObtenerTurnosPorMedicoQuery request, CancellationToken cancellationToken = default)
    {
        var turnos = await _turnoRepository.ObtenerPorMedicoAsync(request.IdMedico, cancellationToken);

        var dtos = turnos.Select(t => new TurnoDto
        {
            IdTurno = t.IdTurno,
            HoraInicio = t.HoraInicio,
            HoraFin = t.HoraFin,
            Estado = t.Estado,
            IdPaciente = t.IdPaciente,
            NombrePaciente = t.Paciente?.ObtenerNombreCompleto(),
            IdMedico = t.IdMedico,
            NombreMedico = t.Medico?.ObtenerNombreCompleto(),
            IdConsultorio = t.IdConsultorio,
            NumeroConsultorio = t.Consultorio?.Numero
        }).ToList();

        return Result.Success<IReadOnlyList<TurnoDto>>(dtos);
    }
}

