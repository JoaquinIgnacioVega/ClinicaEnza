using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.AutenticarPaciente;

public class AutenticarPacienteCommand : IRequest<Result<AutenticarPacienteResponseDto>>
{
    public string Dni { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public AutenticarPacienteCommand()
    {
    }

    public AutenticarPacienteCommand(string dni, string mail, string password)
    {
        Dni = dni;
        Mail = mail;
        Password = password;
    }
}
