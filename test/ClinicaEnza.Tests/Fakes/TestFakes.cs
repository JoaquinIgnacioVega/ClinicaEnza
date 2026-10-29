using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Tests.Fakes;

public class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Entities { get; } = new();

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Entities.Add(entity);
        return Task.CompletedTask;
    }

    public void Delete(T entity) => Entities.Remove(entity);

    public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var compiled = predicate.Compile();
        return Task.FromResult<IReadOnlyList<T>>(Entities.Where(compiled).ToList());
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(Entities.ToList());

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (typeof(T) == typeof(Medico))
        {
            var medico = Entities.Cast<Medico>().FirstOrDefault(m => m.IdPersona == id);
            return Task.FromResult(medico as T);
        }
        if (typeof(T) == typeof(Consultorio))
        {
            var consultorio = Entities.Cast<Consultorio>().FirstOrDefault(c => c.IdConsultorio == id);
            return Task.FromResult(consultorio as T);
        }
        if (typeof(T) == typeof(HorarioAtencion))
        {
            var horario = Entities.Cast<HorarioAtencion>().FirstOrDefault(h => h.IdHorarioAtencion == id);
            return Task.FromResult(horario as T);
        }

        return Task.FromResult<T?>(null);
    }

    public void Update(T entity) { }
}

public class FakeHorarioAtencionRepository : IHorarioAtencionRepository
{
    private readonly FakeRepository<HorarioAtencion> _repo;

    public FakeHorarioAtencionRepository(FakeRepository<HorarioAtencion> repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<HorarioAtencion>> ObtenerPorMedicoAsync(int idMedico, CancellationToken cancellationToken = default)
    {
        var list = _repo.Entities.Where(h => h.IdMedico == idMedico).ToList();
        return Task.FromResult<IReadOnlyList<HorarioAtencion>>(list);
    }

    public Task<HorarioAtencion?> ObtenerPorMedicoYHorarioAsync(int idMedico, DayOfWeek diaSemana, TimeSpan hora, TimeSpan horaFin, CancellationToken cancellationToken = default)
    {
        var item = _repo.Entities.FirstOrDefault(h => h.IdMedico == idMedico && h.DiaSemana == diaSemana && h.HoraDesde <= hora && horaFin <= h.HoraHasta);
        return Task.FromResult(item);
    }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }
    public int CommitCallCount { get; private set; }
    public int RollbackCallCount { get; private set; }
    public bool ThrowOnSave { get; set; }

    public IUsuarioRepository Usuarios => throw new NotImplementedException();
    public IMedicoRepository Medicos => throw new NotImplementedException();

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        CommitCallCount++;
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        RollbackCallCount++;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnSave)
            throw new InvalidOperationException("Error de persistencia simulado.");

        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}
