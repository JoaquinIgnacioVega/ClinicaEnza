using System;

namespace ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

public class HorarioAtencionDto
{
    public int IdHorarioAtencion { get; set; }
    public int IdMedico { get; set; }
    public string NombreMedico { get; set; } = string.Empty;
    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraDesde { get; set; }
    public TimeSpan HoraHasta { get; set; }
    public int IdConsultorio { get; set; }
    public string NumeroConsultorio { get; set; } = string.Empty;
}
