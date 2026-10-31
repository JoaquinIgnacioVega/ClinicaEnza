namespace ClinicaEnza.Application.Features.Pacientes.DTOs;

public class PrescripcionMedicaDto
{
    public int IdPrescripcion { get; set; }
    public int IdHistoriaClinica { get; set; }
    public int IdPaciente { get; set; }
    public DateTime Fecha { get; set; }
    public string Medicamento { get; set; } = string.Empty;
    public int IdMedico { get; set; }
    public string NombreMedico { get; set; } = string.Empty;
    public string MatriculaMedico { get; set; } = string.Empty;
}
