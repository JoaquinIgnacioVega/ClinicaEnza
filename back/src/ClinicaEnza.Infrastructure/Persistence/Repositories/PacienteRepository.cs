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

    public async Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Dni == dni, cancellationToken);
    }

    public async Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.HistoriaClinica)
                .ThenInclude(h => h!.Prescripciones)
            .FirstOrDefaultAsync(p => p.IdPersona == idPaciente, cancellationToken);
    }
}

