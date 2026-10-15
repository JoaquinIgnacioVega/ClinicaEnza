using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Medicos.DTOs;

public class MedicoDto
{
    public int IdMedico { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Apellido.ToUpper()}, {Nombre}";
    public string Matricula { get; set; } = string.Empty;
    public Especialidad Especialidad { get; set; }
    public string EspecialidadNombre => Especialidad.ToString();
    public string Mail { get; set; } = string.Empty;
    public string FichaProfesional => $"Dr/a. {NombreCompleto} - MP: {Matricula} ({Especialidad})";
}

