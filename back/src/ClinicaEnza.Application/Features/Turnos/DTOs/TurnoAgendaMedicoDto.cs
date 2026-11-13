using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.DTOs;

public class TurnoAgendaMedicoDto
{
    public int IdTurno { get; set; }
    public DateTime HoraInicio { get; set; }
    public DateTime HoraFin { get; set; }
    public EstadoTurno Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();

    public int IdPaciente { get; set; }
    public string NombreCompletoPaciente { get; set; } = string.Empty;
    public string DniPaciente { get; set; } = string.Empty;
    public string ObraSocialPaciente { get; set; } = string.Empty;
    public int EdadPaciente { get; set; }

    public int IdConsultorio { get; set; }
    public string UbicacionConsultorio { get; set; } = string.Empty;
}

