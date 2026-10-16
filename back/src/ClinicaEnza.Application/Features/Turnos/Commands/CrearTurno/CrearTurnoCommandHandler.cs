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
    private readonly IRepository<Consultorio> _consultorioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTurnoCommandHandler(
        ITurnoRepository turnoRepository,
        IRepository<Paciente> pacienteRepository,
        IRepository<Medico> medicoRepository,
        IRepository<Consultorio> consultorioRepository,
        IUnitOfWork unitOfWork)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
        _consultorioRepository = consultorioRepository;
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

        var consultorio = await _consultorioRepository.GetByIdAsync(request.IdConsultorio, cancellationToken);
        if (consultorio == null)
            return Result.Failure<TurnoDto>($"No se encontró el consultorio con ID {request.IdConsultorio}.");

        var horaFinEstimada = request.HoraInicio.AddMinutes(20);

        var superpuestosMedico = await _turnoRepository.ObtenerSuperpuestosAsync(
            request.IdMedico, request.HoraInicio, horaFinEstimada, null, cancellationToken);

        if (superpuestosMedico.Any())
            throw new TurnoSuperpuestoException(request.IdMedico, request.HoraInicio);

        var superpuestosConsultorio = await _turnoRepository.ObtenerSuperpuestosConsultorioAsync(
            request.IdConsultorio, request.HoraInicio, horaFinEstimada, null, cancellationToken);

        if (superpuestosConsultorio.Any())
            return Result.Failure<TurnoDto>($"El consultorio '{consultorio.Numero}' ya se encuentra ocupado en ese horario.");

        var nuevoTurno = new Turno(request.HoraInicio, paciente, medico, consultorio);

        await _turnoRepository.AddAsync(nuevoTurno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
