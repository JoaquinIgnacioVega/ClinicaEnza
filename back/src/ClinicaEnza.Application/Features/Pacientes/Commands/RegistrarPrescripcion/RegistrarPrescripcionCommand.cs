using System.Text.Json.Serialization;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;

public class RegistrarPrescripcionCommand : IRequest<Result<PrescripcionMedicaDto>>
{
    [JsonIgnore]
    public int IdPaciente { get; set; }

    public int IdMedico { get; set; }
    public string Medicamento { get; set; } = string.Empty;

    public RegistrarPrescripcionCommand() { }

    public RegistrarPrescripcionCommand(int idPaciente, int idMedico, string medicamento)
    {
        IdPaciente = idPaciente;
        IdMedico = idMedico;
        Medicamento = medicamento;
    }
}
