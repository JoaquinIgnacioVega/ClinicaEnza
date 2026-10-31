using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPrescripcionesPorPaciente;

public record ObtenerPrescripcionesPorPacienteQuery(int IdPaciente)
    : IRequest<Result<IEnumerable<PrescripcionMedicaDto>>>;
