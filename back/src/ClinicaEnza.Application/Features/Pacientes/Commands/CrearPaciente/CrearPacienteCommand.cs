using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;

public class CrearPacienteCommand : IRequest<Result<PacienteDto>>
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
}

