using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.Commands.ModificarMedico;

public class ModificarMedicoCommandHandler : IRequestHandler<ModificarMedicoCommand, Result<MedicoDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ModificarMedicoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<MedicoDto>> Handle(ModificarMedicoCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas de entrada
        if (request.IdMedico <= 0)
            return Result.Failure<MedicoDto>("El ID del médico debe ser válido.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<MedicoDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<MedicoDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<MedicoDto>("El correo electrónico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Matricula))
            return Result.Failure<MedicoDto>("La matrícula es obligatoria.");

        if (!Enum.IsDefined(typeof(Especialidad), request.Especialidad))
            return Result.Failure<MedicoDto>("La especialidad no es válida.");

        // 2. Verificar existencia del médico
        var medico = await _unitOfWork.Medicos.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<MedicoDto>($"No se encontró el médico con ID {request.IdMedico}.");

        // 3. Verificar que la matrícula no pertenezca a otro profesional
        if (await _unitOfWork.Medicos.ExisteMatriculaAsync(request.Matricula.Trim(), request.IdMedico, cancellationToken))
            return Result.Failure<MedicoDto>($"Ya existe otro médico registrado con la matrícula {request.Matricula}.");

        // 4. Actualizar estado y persistir con transacción
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            medico.ActualizarInformacion(
                request.Nombre.Trim(),
                request.Apellido.Trim(),
                request.Mail.Trim(),
                request.Especialidad,
                request.Matricula.Trim());

            _unitOfWork.Medicos.Update(medico);
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
