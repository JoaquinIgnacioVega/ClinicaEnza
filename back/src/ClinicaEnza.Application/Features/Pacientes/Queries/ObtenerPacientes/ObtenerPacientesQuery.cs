using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;

public record ObtenerPacientesQuery : IRequest<Result<IReadOnlyList<PacienteDto>>>;

