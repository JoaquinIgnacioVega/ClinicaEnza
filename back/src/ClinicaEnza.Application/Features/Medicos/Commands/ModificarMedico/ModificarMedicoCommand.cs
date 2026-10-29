using System.Text.Json.Serialization;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.Commands.ModificarMedico;

public class ModificarMedicoCommand : IRequest<Result<MedicoDto>>
{
    [JsonIgnore]
    public int IdMedico { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public Especialidad Especialidad { get; set; }
    public string Matricula { get; set; } = string.Empty;

    public ModificarMedicoCommand() { }

    public ModificarMedicoCommand(int idMedico, string nombre, string apellido, string mail, Especialidad especialidad, string matricula)
    {
        IdMedico = idMedico;
        Nombre = nombre;
        Apellido = apellido;
        Mail = mail;
        Especialidad = especialidad;
        Matricula = matricula;
    }
}
