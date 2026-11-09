using System.Collections.Generic;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Turnos.DTOs;

namespace ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorPaciente;

public record ObtenerTurnosPorPacienteQuery(int IdPaciente, bool SoloPendientes = true) : IRequest<Result<IReadOnlyList<TurnoDto>>>;

