using System;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.AutenticarPaciente;

public class AutenticarPacienteCommandHandler : IRequestHandler<AutenticarPacienteCommand, Result<AutenticarPacienteResponseDto>>
{
    private const string ErrorCredencialesInvalidas = "Los datos ingresados no coinciden con ningún paciente registrado.";

    private readonly IPacienteRepository _pacienteRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AutenticarPacienteCommandHandler(
        IPacienteRepository pacienteRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<Result<AutenticarPacienteResponseDto>> Handle(
        AutenticarPacienteCommand request,
        CancellationToken cancellationToken = default)
    {
        var dniLimpio = request.Dni?.Trim() ?? string.Empty;
        var mailLimpio = request.Mail?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dniLimpio) || string.IsNullOrWhiteSpace(mailLimpio) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<AutenticarPacienteResponseDto>(ErrorCredencialesInvalidas);
        }

        var paciente = await _pacienteRepository.ObtenerPorDniAsync(dniLimpio, cancellationToken);
        if (paciente == null || string.IsNullOrWhiteSpace(paciente.PasswordHash) ||
            string.IsNullOrWhiteSpace(paciente.Mail) ||
            !string.Equals(paciente.Mail.Trim(), mailLimpio, StringComparison.OrdinalIgnoreCase) ||
            !_passwordHasher.VerifyPassword(paciente.PasswordHash, request.Password))
        {
            return Result.Failure<AutenticarPacienteResponseDto>(ErrorCredencialesInvalidas);
        }

        var token = _jwtTokenGenerator.GenerarTokenPaciente(paciente);

        var pacienteDto = new PacienteDto
        {
            IdPaciente = paciente.IdPersona,
            Nombre = paciente.Nombre,
            Apellido = paciente.Apellido,
            Dni = paciente.Dni,
            FechaNacimiento = paciente.FechaNacimiento,
            Edad = paciente.CalcularEdad(),
            Mail = paciente.Mail,
            ObraSocial = paciente.ObraSocial,
            Peso = paciente.Peso,
            Altura = paciente.Altura,
            IdHistoriaClinica = paciente.HistoriaClinica?.IdHistoriaClinica
        };

        return Result.Success(new AutenticarPacienteResponseDto
        {
            Token = token,
            Paciente = pacienteDto,
            RequiereCambioPassword = paciente.DebeCambiarPassword
        });
    }
}
