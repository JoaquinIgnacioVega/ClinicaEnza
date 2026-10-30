namespace ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

public class SlotDisponibleDto
{
    public DateTime HoraInicio { get; set; }
    public DateTime HoraFin { get; set; }
    public int IdConsultorio { get; set; }
    public string UbicacionConsultorio { get; set; } = string.Empty;
}
