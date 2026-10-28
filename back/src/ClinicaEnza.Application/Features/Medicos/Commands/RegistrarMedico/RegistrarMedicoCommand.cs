using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.Commands.RegistrarMedico;

public class RegistrarMedicoCommand : IRequest<Result<MedicoDto>>
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public Especialidad Especialidad { get; set; }
    public DateTime FechaInicio { get; set; }
}
