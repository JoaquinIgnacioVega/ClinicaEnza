using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.CambiarPasswordPaciente;

public sealed class CambiarPasswordPacienteCommandHandler
    : IRequestHandler<CambiarPasswordPacienteCommand, Result<bool>>
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarPasswordPacienteCommandHandler(
        IPacienteRepository pacienteRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _pacienteRepository = pacienteRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        CambiarPasswordPacienteCommand request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NuevaPassword) || request.NuevaPassword.Length < 4)
            return Result.Failure<bool>("La nueva contraseña debe tener al menos 4 caracteres.");

        var paciente = await _pacienteRepository.GetByIdAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
            return Result.Failure<bool>("No se encontró el paciente.");

        paciente.CambiarPassword(_passwordHasher.HashPassword(request.NuevaPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
