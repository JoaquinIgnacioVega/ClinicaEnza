using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IMedicoRepository : IRepository<Medico>
{
    Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default);
    Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default);
}
