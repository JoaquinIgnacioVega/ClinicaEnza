using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Commands.MarcarTurnoRealizado;

public record MarcarTurnoRealizadoCommand(
    int IdTurno,
    string? MedicamentoRecetado = null) : IRequest<Result<TurnoDto>>;
