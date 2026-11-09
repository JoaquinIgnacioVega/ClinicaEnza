using System;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurnoPaciente;

public class CancelarTurnoPacienteCommandHandler : IRequestHandler<CancelarTurnoPacienteCommand, Result<TurnoDto>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public CancelarTurnoPacienteCommandHandler(
        ITurnoRepository turnoRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
    {
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
    }

    public async Task<Result<TurnoDto>> Handle(CancelarTurnoPacienteCommand request, CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.GetByIdAsync(request.IdTurno, cancellationToken);
        if (turno == null)
        {
            return Result.Failure<TurnoDto>("El turno solicitado no existe.");
        }

        try
        {
            turno.CancelarPorPaciente(request.IdPaciente, _dateTimeService.Now);
        }
        catch (ReglaDeNegocioException ex)
        {
            return Result.Failure<TurnoDto>(ex.Message);
        }

        _turnoRepository.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var turnoConDetalle = await _turnoRepository.ObtenerDetallePorIdAsync(turno.IdTurno, cancellationToken) ?? turno;
        var turnoDto = new TurnoDto
        {
            IdTurno = turnoConDetalle.IdTurno,
            HoraInicio = turnoConDetalle.HoraInicio,
            HoraFin = turnoConDetalle.HoraFin,
            Estado = turnoConDetalle.Estado,
            IdPaciente = turnoConDetalle.IdPaciente,
            NombrePaciente = turnoConDetalle.Paciente?.ObtenerNombreCompleto(),
            DniPaciente = turnoConDetalle.Paciente?.Dni,
            ObraSocialPaciente = turnoConDetalle.Paciente?.ObraSocial,
            EdadPaciente = turnoConDetalle.Paciente?.CalcularEdad(),
            IdMedico = turnoConDetalle.IdMedico,
            NombreMedico = turnoConDetalle.Medico?.ObtenerNombreCompleto(),
            IdConsultorio = turnoConDetalle.IdConsultorio,
            NumeroConsultorio = turnoConDetalle.Consultorio?.Numero,
            UbicacionConsultorio = turnoConDetalle.Consultorio?.ObtenerUbicacion()
        };

        return Result.Success(turnoDto);
    }
}

