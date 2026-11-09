using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Repositories;

public class HorarioAtencionRepository : IHorarioAtencionRepository
{
    private readonly AppDbContext _context;

    public HorarioAtencionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HorarioAtencion?> ObtenerPorMedicoYHorarioAsync(
        int idMedico,
        DayOfWeek diaSemana,
        TimeSpan hora,
        TimeSpan horaFin,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _context.HorariosAtencion
            .AsNoTracking()
            .Include(h => h.Medico)
            .Include(h => h.Consultorio)
            .Where(h => h.IdMedico == idMedico && h.DiaSemana == diaSemana)
            .ToListAsync(cancellationToken);

        return horarios
            .FirstOrDefault(h => h.HoraDesde <= hora && horaFin <= h.HoraHasta);
    }

    public async Task<IReadOnlyList<HorarioAtencion>> ObtenerPorMedicoAsync(
        int idMedico,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _context.HorariosAtencion
            .AsNoTracking()
            .Include(h => h.Medico)
            .Include(h => h.Consultorio)
            .Where(h => h.IdMedico == idMedico)
            .ToListAsync(cancellationToken);

        return horarios
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraDesde)
            .ToList();
    }
}
