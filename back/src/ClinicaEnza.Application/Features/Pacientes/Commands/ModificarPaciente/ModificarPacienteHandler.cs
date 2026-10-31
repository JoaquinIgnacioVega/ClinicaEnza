using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.ModificarPaciente;

public class ModificarPacienteHandler : IRequestHandler<ModificarPacienteCommand, Result<PacienteDto>>
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarPacienteHandler(IPacienteRepository pacienteRepository, IUnitOfWork unitOfWork)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PacienteDto>> Handle(ModificarPacienteCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas de entrada
        if (request.IdPaciente <= 0)
            return Result.Failure<PacienteDto>("El ID del paciente debe ser válido.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<PacienteDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<PacienteDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<PacienteDto>("El correo electrónico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.ObraSocial))
            return Result.Failure<PacienteDto>("La obra social es obligatoria.");

        if (request.Peso <= 0)
            return Result.Failure<PacienteDto>("El peso debe ser mayor a cero.");

        if (request.Altura <= 0)
            return Result.Failure<PacienteDto>("La altura debe ser mayor a cero.");

        // 2. Buscar paciente existente
        var paciente = await _pacienteRepository.GetByIdAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
            return Result.Failure<PacienteDto>($"No se encontró el paciente con ID {request.IdPaciente}.");

        // 3. Aplicar métodos de dominio y persistir con transacción
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            paciente.ActualizarDatosPersonales(
                request.Nombre.Trim(),
                request.Apellido.Trim(),
                request.Mail.Trim(),
                request.ObraSocial.Trim());

            paciente.ActualizarMedidas(request.Peso, request.Altura);

            _pacienteRepository.Update(paciente);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var pacienteDto = new PacienteDto
            {
                IdPaciente = paciente.IdPersona,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                Dni = paciente.Dni,
                FechaNacimiento = paciente.FechaNacimiento,
                Mail = paciente.Mail,
                ObraSocial = paciente.ObraSocial,
                Peso = paciente.Peso,
                Altura = paciente.Altura,
                IdHistoriaClinica = paciente.HistoriaClinica?.IdHistoriaClinica
            };

            return Result.Success(pacienteDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

