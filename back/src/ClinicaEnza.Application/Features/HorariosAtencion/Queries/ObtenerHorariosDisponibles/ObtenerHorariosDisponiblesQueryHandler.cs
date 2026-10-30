using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosDisponibles;

public class ObtenerHorariosDisponiblesQueryHandler : IRequestHandler<ObtenerHorariosDisponiblesQuery, Result<IEnumerable<SlotDisponibleDto>>>
{
    private static readonly TimeSpan DuracionSlot = TimeSpan.FromMinutes(20);

    private readonly IHorarioAtencionRepository _horarioRepository;
    private readonly ITurnoRepository _turnoRepository;

    public ObtenerHorariosDisponiblesQueryHandler(
        IHorarioAtencionRepository horarioRepository,
        ITurnoRepository turnoRepository)
    {
        _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
    }

    public async Task<Result<IEnumerable<SlotDisponibleDto>>> Handle(
        ObtenerHorariosDisponiblesQuery request,
        CancellationToken cancellationToken = default)
    {
        var fechaObjetivo = request.Fecha.Date;
        var diaSemana = fechaObjetivo.DayOfWeek;

        // 1. Obtener los horarios de atención del médico para el día de la semana solicitado
        var todosLosHorarios = await _horarioRepository.ObtenerPorMedicoAsync(request.IdMedico, cancellationToken);
        var horariosDelDia = todosLosHorarios
            .Where(h => h.DiaSemana == diaSemana)
            .OrderBy(h => h.HoraDesde)
            .ToList();

        if (horariosDelDia.Count == 0)
        {
            return Result.Success(Enumerable.Empty<SlotDisponibleDto>());
        }

        // 2. Obtener los turnos activos (Estado != Cancelado) del médico en esa fecha
        var turnosMedico = await _turnoRepository.ObtenerPorMedicoAsync(request.IdMedico, cancellationToken);
        var turnosActivosDelDia = turnosMedico
            .Where(t => t.HoraInicio.Date == fechaObjetivo && t.Estado != EstadoTurno.Cancelado)
            .ToList();

        // 3. Generar intervalos de 20 minutos y filtrar pasados o solapados
        var ahora = DateTime.Now;
        var slotsDisponibles = new List<SlotDisponibleDto>();

        foreach (var horario in horariosDelDia)
        {
            var horaActual = horario.HoraDesde;
            while (horaActual.Add(DuracionSlot) <= horario.HoraHasta)
            {
                var slotInicio = fechaObjetivo.Add(horaActual);
                var slotFin = slotInicio.Add(DuracionSlot);

                var estaEnElPasado = slotInicio <= ahora;
                var seSolapaConTurno = turnosActivosDelDia.Any(
                    t => t.HoraInicio < slotFin && slotInicio < t.HoraFin);

                if (!estaEnElPasado && !seSolapaConTurno)
                {
                    slotsDisponibles.Add(new SlotDisponibleDto
                    {
                        HoraInicio = slotInicio,
                        HoraFin = slotFin,
                        IdConsultorio = horario.IdConsultorio,
                        UbicacionConsultorio = horario.Consultorio?.ObtenerUbicacion() ?? string.Empty
                    });
                }

                horaActual = horaActual.Add(DuracionSlot);
            }
        }

        return Result.Success<IEnumerable<SlotDisponibleDto>>(slotsDisponibles);
    }
}
