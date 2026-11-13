using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;

namespace ClinicaEnza.Application.Features.Consultorios.Commands.ModificarConsultorio;
public class ModificarConsultorioCommand : IRequest<Result<ConsultorioDto>>
{
    public int IdConsultorio { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Sector { get; set; } = string.Empty;
}
