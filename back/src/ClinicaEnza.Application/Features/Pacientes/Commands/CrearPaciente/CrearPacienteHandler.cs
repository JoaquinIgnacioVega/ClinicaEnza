using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;

public class CrearPacienteHandler : IRequestHandler<CrearPacienteCommand, Result<PacienteDto>>
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPacienteHandler(IPacienteRepository pacienteRepository, IUnitOfWork unitOfWork)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PacienteDto>> Handle(CrearPacienteCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas de entrada
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<PacienteDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<PacienteDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Dni))
            return Result.Failure<PacienteDto>("El DNI es obligatorio.");

        if (request.FechaNacimiento == default)
            return Result.Failure<PacienteDto>("La fecha de nacimiento es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<PacienteDto>("El correo electrónico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.ObraSocial))
            return Result.Failure<PacienteDto>("La obra social es obligatoria.");

        if (request.Peso <= 0)
            return Result.Failure<PacienteDto>("El peso debe ser mayor a cero.");

        if (request.Altura <= 0)
            return Result.Failure<PacienteDto>("La altura debe ser mayor a cero.");

        // 2. Validar que no exista otro paciente con el mismo DNI
        var pacienteExistente = await _pacienteRepository.ObtenerPorDniAsync(request.Dni.Trim(), cancellationToken);
        if (pacienteExistente != null)
            return Result.Failure<PacienteDto>($"Ya existe un paciente registrado con el DNI {request.Dni}.");

        // 3. Iniciar transacción explícita y persistir
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var paciente = new Paciente(
                request.Nombre.Trim(),
                request.Apellido.Trim(),
                request.Dni.Trim(),
                request.FechaNacimiento,
                request.Mail.Trim(),
                request.ObraSocial.Trim(),
                request.Peso,
                request.Altura);

            await _pacienteRepository.AddAsync(paciente, cancellationToken);
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

