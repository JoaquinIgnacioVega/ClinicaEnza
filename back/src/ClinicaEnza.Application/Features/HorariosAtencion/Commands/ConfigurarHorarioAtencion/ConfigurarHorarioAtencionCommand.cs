using System;
using System.Text.Json.Serialization;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;

public class ConfigurarHorarioAtencionCommand : IRequest<Result<HorarioAtencionDto>>
{
    [JsonIgnore]
    public int IdMedico { get; set; }

    public int IdConsultorio { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraDesde { get; set; }
    public TimeSpan HoraHasta { get; set; }

    public ConfigurarHorarioAtencionCommand() { }

    public ConfigurarHorarioAtencionCommand(
        int idMedico,
        int idConsultorio,
        DayOfWeek diaSemana,
        TimeSpan horaDesde,
        TimeSpan horaHasta)
    {
        IdMedico = idMedico;
        IdConsultorio = idConsultorio;
        DiaSemana = diaSemana;
        HoraDesde = horaDesde;
        HoraHasta = horaHasta;
    }

    public ConfigurarHorarioAtencionCommand(
        int idMedico,
        DayOfWeek diaSemana,
        TimeSpan horaDesde,
        TimeSpan horaHasta,
        int idConsultorio)
        : this(idMedico, idConsultorio, diaSemana, horaDesde, horaHasta)
    {
    }
}
