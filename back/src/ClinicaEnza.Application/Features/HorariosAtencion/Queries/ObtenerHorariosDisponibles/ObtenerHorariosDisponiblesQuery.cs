using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosDisponibles;

public record ObtenerHorariosDisponiblesQuery(int IdMedico, DateTime Fecha)
    : IRequest<Result<IEnumerable<SlotDisponibleDto>>>;
