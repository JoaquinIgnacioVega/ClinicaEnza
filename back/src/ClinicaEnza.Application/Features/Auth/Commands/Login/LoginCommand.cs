using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Auth.DTOs;

namespace ClinicaEnza.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Identificador { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public LoginCommand() { }

    public LoginCommand(string identificador, string password)
    {
        Identificador = identificador;
        Password = password;
    }
}

