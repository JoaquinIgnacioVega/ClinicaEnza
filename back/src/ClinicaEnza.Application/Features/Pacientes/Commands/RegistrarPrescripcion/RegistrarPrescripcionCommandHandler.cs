using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;

public class RegistrarPrescripcionCommandHandler : IRequestHandler<RegistrarPrescripcionCommand, Result<PrescripcionMedicaDto>>
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMedicoRepository _medicoRepository;
    private readonly IRepository<PrescripcionMedica> _prescripcionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarPrescripcionCommandHandler(
        IPacienteRepository pacienteRepository,
        IMedicoRepository medicoRepository,
        IRepository<PrescripcionMedica> prescripcionRepository,
        IUnitOfWork unitOfWork)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
        _prescripcionRepository = prescripcionRepository ?? throw new ArgumentNullException(nameof(prescripcionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PrescripcionMedicaDto>> Handle(
        RegistrarPrescripcionCommand request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Medicamento))
            return Result.Failure<PrescripcionMedicaDto>("El nombre del medicamento es obligatorio.");

        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<PrescripcionMedicaDto>($"No se encontró el médico con ID {request.IdMedico}.");

        var paciente = await _pacienteRepository.ObtenerConHistoriaClinicaAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
            return Result.Failure<PrescripcionMedicaDto>($"No se encontró el paciente con ID {request.IdPaciente}.");

        if (paciente.HistoriaClinica == null)
            return Result.Failure<PrescripcionMedicaDto>($"No se encontró la historia clínica del paciente con ID {request.IdPaciente}.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var prescripcion = paciente.HistoriaClinica.AgregarPrescripcion(
                request.IdMedico,
                request.Medicamento,
                medico);

            await _prescripcionRepository.AddAsync(prescripcion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var dto = new PrescripcionMedicaDto
            {
                IdPrescripcion = prescripcion.IdPrescripcion,
                IdHistoriaClinica = paciente.HistoriaClinica.IdHistoriaClinica,
                IdPaciente = paciente.IdPersona,
                Fecha = prescripcion.Fecha,
                Medicamento = prescripcion.Medicamento,
                IdMedico = medico.IdPersona,
                NombreMedico = medico.ObtenerNombreCompleto(),
                MatriculaMedico = medico.Matricula
            };

            return Result.Success(dto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
