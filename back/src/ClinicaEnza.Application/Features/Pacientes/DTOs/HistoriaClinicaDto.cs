namespace ClinicaEnza.Application.Features.Pacientes.DTOs;

public class HistoriaClinicaDto
{
    public int IdHistoriaClinica { get; set; }
    public int IdPaciente { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public string DniPaciente { get; set; } = string.Empty;
    public string MailPaciente { get; set; } = string.Empty;
    public int EdadPaciente { get; set; }
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public List<string> MedicamentosRecetados { get; set; } = new();
    public List<PrescripcionMedicaDto> Prescripciones { get; set; } = new();
}
