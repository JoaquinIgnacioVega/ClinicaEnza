using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Application.Features.Turnos.Commands.ReagendarTurno;

public class ReagendarTurnoCommandHandler : IRequestHandler<ReagendarTurnoCommand, Result<TurnoDto>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IHorarioAtencionRepository _horarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReagendarTurnoCommandHandler(
        ITurnoRepository turnoRepository,
        IHorarioAtencionRepository horarioRepository,
        IUnitOfWork unitOfWork)
    {
        _turnoRepository = turnoRepository;
        _horarioRepository = horarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TurnoDto>> Handle(ReagendarTurnoCommand request, CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.GetByIdAsync(request.IdTurno, cancellationToken);
        if (turno == null)
            return Result.Failure<TurnoDto>($"No se encontró el turno con ID {request.IdTurno}.");

        var nuevaHoraFin = request.NuevaHoraInicio.AddMinutes(20);
        var horario = await _horarioRepository.ObtenerPorMedicoYHorarioAsync(
            turno.IdMedico,
            request.NuevaHoraInicio.DayOfWeek,
            request.NuevaHoraInicio.TimeOfDay,
            nuevaHoraFin.TimeOfDay,
            cancellationToken);

        if (horario == null)
        {
            return Result.Failure<TurnoDto>(
                "El nuevo horario no está incluido en la agenda de atención del médico.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var superpuestosMedico = await _turnoRepository.ObtenerSuperpuestosAsync(
                turno.IdMedico, request.NuevaHoraInicio, nuevaHoraFin, turno.IdTurno, cancellationToken);

            if (superpuestosMedico.Any())
                throw new TurnoSuperpuestoException(turno.IdMedico, request.NuevaHoraInicio);

            var superpuestosConsultorio = await _turnoRepository.ObtenerSuperpuestosConsultorioAsync(
                horario.IdConsultorio, request.NuevaHoraInicio, nuevaHoraFin, turno.IdTurno, cancellationToken);

            if (superpuestosConsultorio.Any())
            {
                throw new TurnoSuperpuestoException(
                    $"El consultorio '{horario.Consultorio.Numero}' ya se encuentra ocupado en ese horario.");
            }

            var superpuestosPaciente = await _turnoRepository.ObtenerSuperpuestosPacienteAsync(
                turno.IdPaciente, request.NuevaHoraInicio, nuevaHoraFin, turno.IdTurno, cancellationToken);

            if (superpuestosPaciente.Any())
            {
                throw new TurnoSuperpuestoException(
                    "El paciente ya tiene otro turno asignado en ese horario.");
            }

            turno.Reagendar(request.NuevaHoraInicio, horario.Consultorio);
            _turnoRepository.Update(turno);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new TurnoDto
            {
                IdTurno = turno.IdTurno,
                HoraInicio = turno.HoraInicio,
                HoraFin = turno.HoraFin,
                Estado = turno.Estado,
                IdPaciente = turno.IdPaciente,
                NombrePaciente = turno.Paciente?.ObtenerNombreCompleto(),
                IdMedico = turno.IdMedico,
                NombreMedico = turno.Medico?.ObtenerNombreCompleto(),
                IdConsultorio = turno.IdConsultorio,
                NumeroConsultorio = turno.Consultorio?.Numero
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
