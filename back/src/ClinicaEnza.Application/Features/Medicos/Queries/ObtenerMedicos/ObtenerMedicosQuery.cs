using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;

public record ObtenerMedicosQuery(Especialidad? Especialidad = null) : IRequest<Result<IEnumerable<MedicoDto>>>;
