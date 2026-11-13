using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Repositories;

public class TurnoRepository : Repository<Turno>, ITurnoRepository
{
    public TurnoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Turno>> ObtenerSuperpuestosAsync(
        int idMedico,
        DateTime inicio,
        DateTime fin,
        int? turnoIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(t => t.IdMedico == idMedico && t.Estado != EstadoTurno.Cancelado)
            .Where(t => t.HoraInicio < fin && inicio < t.HoraFin);

        if (turnoIdExcluir.HasValue)
        {
            query = query.Where(t => t.IdTurno != turnoIdExcluir.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> ObtenerSuperpuestosConsultorioAsync(
        int idConsultorio,
        DateTime inicio,
        DateTime fin,
        int? turnoIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(t => t.IdConsultorio == idConsultorio && t.Estado != EstadoTurno.Cancelado)
            .Where(t => t.HoraInicio < fin && inicio < t.HoraFin);

        if (turnoIdExcluir.HasValue)
        {
            query = query.Where(t => t.IdTurno != turnoIdExcluir.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> ObtenerSuperpuestosPacienteAsync(
        int idPaciente,
        DateTime inicio,
        DateTime fin,
        int? turnoIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(t => t.IdPaciente == idPaciente && t.Estado != EstadoTurno.Cancelado)
            .Where(t => t.HoraInicio < fin && inicio < t.HoraFin);

        if (turnoIdExcluir.HasValue)
        {
            query = query.Where(t => t.IdTurno != turnoIdExcluir.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> ObtenerPorMedicoAsync(
        int idMedico,
        DateTime? fecha = null,
        EstadoTurno? estado = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _dbSet.AsNoTracking()
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.Consultorio)
            .Where(t => t.IdMedico == idMedico)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var fechaInicio = fecha.Value.Date;
            var fechaFin = fechaInicio.AddDays(1);
            consulta = consulta.Where(t => t.HoraInicio >= fechaInicio && t.HoraInicio < fechaFin);
        }

        if (estado.HasValue)
            consulta = consulta.Where(t => t.Estado == estado.Value);

        return await consulta
            .OrderBy(t => t.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> ObtenerAgendaMedicoAsync(
        int idMedico,
        DateTime? fecha = null,
        EstadoTurno? estado = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Turno> query = _dbSet.AsNoTracking()
            .Include(t => t.Paciente)
            .Include(t => t.Consultorio)
            .Where(t => t.IdMedico == idMedico);

        if (fecha.HasValue)
        {
            var fechaInicio = fecha.Value.Date;
            var fechaFin = fechaInicio.AddDays(1);
            query = query.Where(t => t.HoraInicio >= fechaInicio && t.HoraInicio < fechaFin);
        }

        if (estado.HasValue)
        {
            query = query.Where(t => t.Estado == estado.Value);
        }

        return await query
            .OrderBy(t => t.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Turno>> BuscarTurnosAsync(
        DateTime? fecha,
        int? idPaciente,
        int? idMedico,
        EstadoTurno? estado,
        CancellationToken cancellationToken = default)
    {
        var consulta = _dbSet.AsNoTracking()
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.Consultorio)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var fechaInicio = fecha.Value.Date;
            var fechaFin = fechaInicio.AddDays(1);
            consulta = consulta.Where(t => t.HoraInicio >= fechaInicio && t.HoraInicio < fechaFin);
        }

        if (idPaciente.HasValue)
            consulta = consulta.Where(t => t.IdPaciente == idPaciente.Value);

        if (idMedico.HasValue)
            consulta = consulta.Where(t => t.IdMedico == idMedico.Value);

        if (estado.HasValue)
            consulta = consulta.Where(t => t.Estado == estado.Value);

        return await consulta
            .OrderBy(t => t.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<Turno?> ObtenerDetallePorIdAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.Consultorio)
            .FirstOrDefaultAsync(t => t.IdTurno == idTurno, cancellationToken);
    }
}

