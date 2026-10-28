using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;
using ClinicaEnza.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicaEnza.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Dni == dni, cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorMailAsync(string mail, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Mail.ToLower() == mail.ToLower(), cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Legajo == legajo, cancellationToken);
    }

    public async Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        return await _context.Personas.AnyAsync(p => p.Dni == dni, cancellationToken);
    }

    public async Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default)
    {
        return await _context.Personas.AnyAsync(p => p.Mail.ToLower() == mail.ToLower(), cancellationToken);
    }

    public async Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Legajo == legajo, cancellationToken);
    }
}

