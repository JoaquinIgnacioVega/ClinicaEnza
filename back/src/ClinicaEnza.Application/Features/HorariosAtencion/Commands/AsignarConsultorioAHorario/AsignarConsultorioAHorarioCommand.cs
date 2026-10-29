using System.Text.Json.Serialization;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Commands.AsignarConsultorioAHorario;

public class AsignarConsultorioAHorarioCommand : IRequest<Result<HorarioAtencionDto>>
{
    [JsonIgnore]
    public int IdHorarioAtencion { get; set; }

    public int IdConsultorio { get; set; }

    public AsignarConsultorioAHorarioCommand() { }

    public AsignarConsultorioAHorarioCommand(int idHorarioAtencion, int idConsultorio)
    {
        IdHorarioAtencion = idHorarioAtencion;
        IdConsultorio = idConsultorio;
    }
}
