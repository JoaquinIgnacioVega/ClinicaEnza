using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorPaciente;

public class ObtenerTurnosPorPacienteQueryHandler : IRequestHandler<ObtenerTurnosPorPacienteQuery, Result<IReadOnlyList<TurnoDto>>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;

    public ObtenerTurnosPorPacienteQueryHandler(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository)
    {
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
    }

    public async Task<Result<IReadOnlyList<TurnoDto>>> Handle(ObtenerTurnosPorPacienteQuery request, CancellationToken cancellationToken = default)
    {
        if (request.IdPaciente <= 0)
        {
            return Result.Failure<IReadOnlyList<TurnoDto>>("El ID del paciente debe ser válido.");
        }

        var paciente = await _pacienteRepository.GetByIdAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
        {
            return Result.Failure<IReadOnlyList<TurnoDto>>($"No se encontró el paciente con ID {request.IdPaciente}.");
        }

        var turnos = await _turnoRepository.ObtenerPorPacienteAsync(
            request.IdPaciente,
            request.SoloPendientes,
            cancellationToken);

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

