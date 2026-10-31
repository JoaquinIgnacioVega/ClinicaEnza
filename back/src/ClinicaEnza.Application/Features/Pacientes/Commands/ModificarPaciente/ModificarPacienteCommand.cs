using System.Text.Json.Serialization;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.ModificarPaciente;

public class ModificarPacienteCommand : IRequest<Result<PacienteDto>>
{
    [JsonIgnore]
    public int IdPaciente { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }

    public ModificarPacienteCommand() { }

    public ModificarPacienteCommand(
        int idPaciente,
        string nombre,
        string apellido,
        string mail,
        string obraSocial,
        decimal peso,
        decimal altura)
    {
        IdPaciente = idPaciente;
        Nombre = nombre;
        Apellido = apellido;
        Mail = mail;
        ObraSocial = obraSocial;
        Peso = peso;
        Altura = altura;
    }
}

