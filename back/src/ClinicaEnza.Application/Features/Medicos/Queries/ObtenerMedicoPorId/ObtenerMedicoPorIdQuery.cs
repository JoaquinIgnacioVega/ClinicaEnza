using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicoPorId;

public record ObtenerMedicoPorIdQuery(int IdMedico) : IRequest<Result<MedicoDto>>;
