using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.DTOs;

public class TurnoDto
{
    public int IdTurno { get; set; }
    public DateTime HoraInicio { get; set; }
    public DateTime HoraFin { get; set; }
    public EstadoTurno Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();

    public int IdPaciente { get; set; }
    public string? NombrePaciente { get; set; }
    public string? DniPaciente { get; set; }
    public string? ObraSocialPaciente { get; set; }
    public int? EdadPaciente { get; set; }

    public int IdMedico { get; set; }
    public string? NombreMedico { get; set; }

    public int IdConsultorio { get; set; }
    public string? NumeroConsultorio { get; set; }
    public string? UbicacionConsultorio { get; set; }
}

