namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IMedicoRepository Medicos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
