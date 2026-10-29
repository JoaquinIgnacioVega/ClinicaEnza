namespace ClinicaEnza.Application.Features.Consultorios.DTOs;

public class ConsultorioDto
{
    public int IdConsultorio { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
}
