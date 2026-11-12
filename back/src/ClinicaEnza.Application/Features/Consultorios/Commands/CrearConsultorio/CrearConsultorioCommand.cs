using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;

namespace ClinicaEnza.Application.Features.Consultorios.Commands.CrearConsultorio;

public class CrearConsultorioCommand : IRequest<Result<ConsultorioDto>>
{
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Sector { get; set; } = string.Empty;
}
