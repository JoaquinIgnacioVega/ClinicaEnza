using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;
namespace ClinicaEnza.Application.Features.Consultorios.Commands.DesactivarConsultorio;
public record DesactivarConsultorioCommand(int IdConsultorio) : IRequest<Result<ConsultorioDto>>;
