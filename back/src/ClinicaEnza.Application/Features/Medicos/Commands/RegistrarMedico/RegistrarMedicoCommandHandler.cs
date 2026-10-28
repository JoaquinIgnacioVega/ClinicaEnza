using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.Commands.RegistrarMedico;

public class RegistrarMedicoCommandHandler : IRequestHandler<RegistrarMedicoCommand, Result<MedicoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarMedicoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<MedicoDto>> Handle(RegistrarMedicoCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones de entrada
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<MedicoDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<MedicoDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Dni))
            return Result.Failure<MedicoDto>("El DNI es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<MedicoDto>("El correo electrónico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Matricula))
            return Result.Failure<MedicoDto>("La matrícula es obligatoria.");

        if (!Enum.IsDefined(typeof(Especialidad), request.Especialidad))
            return Result.Failure<MedicoDto>("La especialidad no es válida.");

        if (request.FechaNacimiento == default)
            return Result.Failure<MedicoDto>("La fecha de nacimiento es obligatoria.");

        if (request.FechaInicio == default)
            return Result.Failure<MedicoDto>("La fecha de inicio es obligatoria.");

        // 2. Verificar matrícula duplicada
        if (await _unitOfWork.Medicos.ExisteMatriculaAsync(request.Matricula.Trim(), cancellationToken))
            return Result.Failure<MedicoDto>($"Ya existe un médico registrado con la matrícula {request.Matricula}.");

        // 3. Verificar DNI duplicado en médicos
        if (await _unitOfWork.Medicos.ExisteDniAsync(request.Dni.Trim(), cancellationToken))
            return Result.Failure<MedicoDto>($"Ya existe un médico registrado con el DNI {request.Dni}.");

        // 4. Transacción
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var medico = new Medico(
                request.Nombre.Trim(),
                request.Apellido.Trim(),
                request.Dni.Trim(),
                request.FechaNacimiento,
                request.Mail.Trim(),
                request.Especialidad,
                request.Matricula.Trim(),
                request.FechaInicio);

            await _unitOfWork.Medicos.AddAsync(medico, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var medicoDto = new MedicoDto
            {
                IdMedico = medico.IdPersona,
                Nombre = medico.Nombre,
                Apellido = medico.Apellido,
                Dni = medico.Dni,
                FechaNacimiento = medico.FechaNacimiento,
                Mail = medico.Mail,
                Matricula = medico.Matricula,
                Especialidad = medico.Especialidad,
                FechaInicio = medico.FechaInicio
            };

            return Result.Success(medicoDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
