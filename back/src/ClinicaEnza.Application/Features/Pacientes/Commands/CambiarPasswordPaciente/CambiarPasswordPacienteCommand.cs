using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.CambiarPasswordPaciente;

public sealed record CambiarPasswordPacienteCommand(int IdPaciente, string NuevaPassword)
    : IRequest<Result<bool>>;
