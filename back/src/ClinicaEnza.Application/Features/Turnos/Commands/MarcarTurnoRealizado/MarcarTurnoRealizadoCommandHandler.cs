using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Turnos.Commands.MarcarTurnoRealizado;

public class MarcarTurnoRealizadoCommandHandler : IRequestHandler<MarcarTurnoRealizadoCommand, Result<TurnoDto>>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMedicoRepository _medicoRepository;
    private readonly IRepository<PrescripcionMedica> _prescripcionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarcarTurnoRealizadoCommandHandler(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IMedicoRepository medicoRepository,
        IRepository<PrescripcionMedica> prescripcionRepository,
        IUnitOfWork unitOfWork)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
        _prescripcionRepository = prescripcionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TurnoDto>> Handle(
        MarcarTurnoRealizadoCommand request,
        CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.GetByIdAsync(request.IdTurno, cancellationToken);
        if (turno == null)
            return Result.Failure<TurnoDto>($"No se encontró el turno con ID {request.IdTurno}.");

        var paciente = await _pacienteRepository.ObtenerConHistoriaClinicaAsync(
            turno.IdPaciente, cancellationToken);
        if (paciente == null)
            return Result.Failure<TurnoDto>($"No se encontró el paciente con ID {turno.IdPaciente}.");

        var medico = await _medicoRepository.GetByIdAsync(turno.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<TurnoDto>($"No se encontró el médico con ID {turno.IdMedico}.");

        if (!string.IsNullOrWhiteSpace(request.MedicamentoRecetado) && paciente.HistoriaClinica == null)
        {
            return Result.Failure<TurnoDto>(
                $"No se encontró la historia clínica del paciente con ID {turno.IdPaciente}.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            turno.MarcarComoRealizado();

            if (!string.IsNullOrWhiteSpace(request.MedicamentoRecetado))
            {
                var prescripcion = paciente.HistoriaClinica!.AgregarPrescripcion(
                    turno.IdMedico,
                    request.MedicamentoRecetado,
                    medico);

                await _prescripcionRepository.AddAsync(prescripcion, cancellationToken);
            }

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
                NombrePaciente = paciente.ObtenerNombreCompleto(),
                IdMedico = turno.IdMedico,
                NombreMedico = medico.ObtenerNombreCompleto(),
                IdConsultorio = turno.IdConsultorio,
                NumeroConsultorio = turno.Consultorio?.Numero,
                UbicacionConsultorio = turno.Consultorio?.ObtenerUbicacion()
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
