using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;

public class CrearTurnoCommandHandler : IRequestHandler<CrearTurnoCommand, Result<TurnoDto>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IRepository<Paciente> _pacienteRepository;
    private readonly IRepository<Medico> _medicoRepository;
    private readonly IHorarioAtencionRepository _horarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTurnoCommandHandler(
        ITurnoRepository turnoRepository,
        IRepository<Paciente> pacienteRepository,
        IRepository<Medico> medicoRepository,
        IHorarioAtencionRepository horarioRepository,
        IUnitOfWork unitOfWork)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
        _horarioRepository = horarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TurnoDto>> Handle(CrearTurnoCommand request, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.GetByIdAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
            return Result.Failure<TurnoDto>($"No se encontró el paciente con ID {request.IdPaciente}.");

        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<TurnoDto>($"No se encontró el médico con ID {request.IdMedico}.");

        var horaFinEstimada = request.HoraInicio.AddMinutes(20);
        var horario = await _horarioRepository.ObtenerPorMedicoYHorarioAsync(
            request.IdMedico,
            request.HoraInicio.DayOfWeek,
            request.HoraInicio.TimeOfDay,
            horaFinEstimada.TimeOfDay,
            cancellationToken);

        if (horario == null)
        {
            return Result.Failure<TurnoDto>(
                $"El médico con ID {request.IdMedico} no tiene un horario de atención que incluya la fecha y hora solicitadas.");
        }

        var consultorio = horario.Consultorio;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var superpuestosMedico = await _turnoRepository.ObtenerSuperpuestosAsync(
                request.IdMedico, request.HoraInicio, horaFinEstimada, null, cancellationToken);

            if (superpuestosMedico.Any())
                throw new TurnoSuperpuestoException(request.IdMedico, request.HoraInicio);

            var superpuestosConsultorio = await _turnoRepository.ObtenerSuperpuestosConsultorioAsync(
                horario.IdConsultorio, request.HoraInicio, horaFinEstimada, null, cancellationToken);

            if (superpuestosConsultorio.Any())
            {
                throw new TurnoSuperpuestoException(
                    $"El consultorio '{consultorio.Numero}' ya se encuentra ocupado en ese horario.");
            }

            var nuevoTurno = new Turno(request.HoraInicio, paciente, medico, consultorio);
            await _turnoRepository.AddAsync(nuevoTurno, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new TurnoDto
            {
                IdTurno = nuevoTurno.IdTurno,
                HoraInicio = nuevoTurno.HoraInicio,
                HoraFin = nuevoTurno.HoraFin,
                Estado = nuevoTurno.Estado,
                IdPaciente = paciente.IdPersona,
                NombrePaciente = paciente.ObtenerNombreCompleto(),
                IdMedico = medico.IdPersona,
                NombreMedico = medico.ObtenerNombreCompleto(),
                IdConsultorio = consultorio.IdConsultorio,
                NumeroConsultorio = consultorio.Numero
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
