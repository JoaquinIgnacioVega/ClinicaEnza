using System.Linq.Expressions;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Repositories;

public class PacienteRepository : Repository<Paciente>, IPacienteRepository
{
    public PacienteRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
            .FirstOrDefaultAsync(p => p.IdPersona == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<Paciente>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Paciente>> FindAsync(Expression<Func<Paciente, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
            .FirstOrDefaultAsync(p => p.Dni == dni, cancellationToken);
    }

    public async Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
                .ThenInclude(h => h!.Prescripciones)
                    .ThenInclude(pr => pr.Medico)
            .FirstOrDefaultAsync(p => p.IdPersona == idPaciente, cancellationToken);
    }
}
