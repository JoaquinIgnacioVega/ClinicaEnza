namespace ClinicaEnza.Application.Features.Pacientes.DTOs;

public class PacienteDto
{
    public int IdPaciente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Apellido.ToUpper()}, {Nombre}";
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Mail { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public int? IdHistoriaClinica { get; set; }
}

