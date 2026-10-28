using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Repositories;

public class MedicoRepository : Repository<Medico>, IMedicoRepository
{
    public MedicoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(m => m.Matricula == matricula, cancellationToken);
    }

    public async Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(m => m.Dni == dni, cancellationToken);
    }
}
