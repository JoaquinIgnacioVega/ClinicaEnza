using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnos;

public class ObtenerTurnosQueryHandler : IRequestHandler<ObtenerTurnosQuery, Result<IReadOnlyList<TurnoDto>>>
{
    private readonly ITurnoRepository _turnoRepository;

    public ObtenerTurnosQueryHandler(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    public async Task<Result<IReadOnlyList<TurnoDto>>> Handle(
        ObtenerTurnosQuery request,
        CancellationToken cancellationToken = default)
    {
        var turnos = await _turnoRepository.BuscarTurnosAsync(
            request.Fecha,
            request.IdPaciente,
            request.IdMedico,
            request.Estado,
            cancellationToken);

        var turnosDto = turnos.Select(turnoRegistrado => new TurnoDto
        {
            IdTurno = turnoRegistrado.IdTurno,
            HoraInicio = turnoRegistrado.HoraInicio,
            HoraFin = turnoRegistrado.HoraFin,
            Estado = turnoRegistrado.Estado,
            IdPaciente = turnoRegistrado.IdPaciente,
            NombrePaciente = turnoRegistrado.Paciente?.ObtenerNombreCompleto(),
            IdMedico = turnoRegistrado.IdMedico,
            NombreMedico = turnoRegistrado.Medico?.ObtenerNombreCompleto(),
            IdConsultorio = turnoRegistrado.IdConsultorio,
            NumeroConsultorio = turnoRegistrado.Consultorio?.Numero
        }).ToList();

        return Result.Success<IReadOnlyList<TurnoDto>>(turnosDto);
    }
}
