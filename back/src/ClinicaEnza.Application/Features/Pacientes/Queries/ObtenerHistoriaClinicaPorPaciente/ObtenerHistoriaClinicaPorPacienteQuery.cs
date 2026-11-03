using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerHistoriaClinicaPorPaciente;

public record ObtenerHistoriaClinicaPorPacienteQuery(int IdPaciente)
    : IRequest<Result<HistoriaClinicaDto>>;
