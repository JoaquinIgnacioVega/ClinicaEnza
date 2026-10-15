using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;

public record ObtenerMedicosQuery : IRequest<Result<IReadOnlyList<MedicoDto>>>;

