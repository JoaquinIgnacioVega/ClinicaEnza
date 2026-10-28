using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorMailAsync(string mail, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default);
    Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default);
    Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default);
    Task<bool> ExisteMailParaOtraPersonaAsync(string mail, int idPersonaExcluir, CancellationToken cancellationToken = default);
}

