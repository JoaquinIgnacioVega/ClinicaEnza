using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;

namespace ClinicaEnza.Application.Features.Consultorios.Queries.ObtenerConsultorios;

public record ObtenerConsultoriosQuery() : IRequest<Result<IReadOnlyList<ConsultorioDto>>>;
