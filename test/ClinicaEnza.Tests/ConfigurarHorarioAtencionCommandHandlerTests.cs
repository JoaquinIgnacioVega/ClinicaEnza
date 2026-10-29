using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;
using ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Domain.Exceptions;
using Xunit;

namespace ClinicaEnza.Tests;

public class ConfigurarHorarioAtencionCommandHandlerTests
{
    private readonly FakeRepository<HorarioAtencion> _horarioRepository;
    private readonly FakeRepository<Medico> _medicoRepository;
    private readonly FakeRepository<Consultorio> _consultorioRepository;
    private readonly FakeHorarioAtencionRepository _customHorarioRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly ConfigurarHorarioAtencionCommandHandler _handler;
    private readonly ObtenerHorariosPorMedicoQueryHandler _queryHandler;

    public ConfigurarHorarioAtencionCommandHandlerTests()
    {
        _horarioRepository = new FakeRepository<HorarioAtencion>();
        _medicoRepository = new FakeRepository<Medico>();
        _consultorioRepository = new FakeRepository<Consultorio>();
        _unitOfWork = new FakeUnitOfWork();
        _customHorarioRepository = new FakeHorarioAtencionRepository(_horarioRepository);

        _handler = new ConfigurarHorarioAtencionCommandHandler(
            _horarioRepository,
            _medicoRepository,
            _consultorioRepository,
            _unitOfWork);

        _queryHandler = new ObtenerHorariosPorMedicoQueryHandler(
            _customHorarioRepository,
            _medicoRepository);
    }

    private Medico CrearMedico(int id = 1)
    {
        var medico = new Medico("Carlos", "Gomez", "20111222", new DateTime(1980, 5, 10), "carlos@test.com", Especialidad.Cardiologia, "MP-100", new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };
        _medicoRepository.Entities.Add(medico);
        return medico;
    }

    private Consultorio CrearConsultorio(int id = 1, string numero = "101")
    {
        var consultorio = new Consultorio(numero, 1, "Ala Norte")
        {
            IdConsultorio = id
        };
        _consultorioRepository.Entities.Add(consultorio);
        return consultorio;
    }

    [Fact]
    public async Task Handle_DatosValidos_ConfiguraHorarioYConfirmaTransaccion()
    {
        // Arrange
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1, "101");
        var command = new ConfigurarHorarioAtencionCommand(
            1, 1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(medico.IdPersona, result.Value.IdMedico);
        Assert.Equal(consultorio.IdConsultorio, result.Value.IdConsultorio);
        Assert.Equal(DayOfWeek.Monday, result.Value.DiaSemana);
        Assert.Equal(new TimeSpan(8, 0, 0), result.Value.HoraDesde);
        Assert.Equal(new TimeSpan(12, 0, 0), result.Value.HoraHasta);
        Assert.Single(_horarioRepository.Entities);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_HoraHastaMenorOIgualQueHoraDesde_RetornaFallo()
    {
        // Arrange
        CrearMedico(1);
        CrearConsultorio(1);
        var command = new ConfigurarHorarioAtencionCommand(
            1, 1, DayOfWeek.Monday, new TimeSpan(12, 0, 0), new TimeSpan(8, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("posterior", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MedicoInexistente_RetornaFalloNotFound()
    {
        // Arrange
        CrearConsultorio(1);
        var command = new ConfigurarHorarioAtencionCommand(
            999, 1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ConsultorioInexistente_RetornaFalloNotFound()
    {
        // Arrange
        CrearMedico(1);
        var command = new ConfigurarHorarioAtencionCommand(
            1, 999, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el consultorio", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_HorarioSuperpuestoMismoMedico_LanzaHorarioAtencionSuperpuestoException()
    {
        // Arrange
        var medico = CrearMedico(1);
        var consultorio1 = CrearConsultorio(1, "101");
        var consultorio2 = CrearConsultorio(2, "102");

        // Horario existente: Lunes 8 a 12 en consultorio 101
        var horarioExistente = new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico, consultorio1);
        _horarioRepository.Entities.Add(horarioExistente);

        // Intento de horario: Lunes 10 a 14 para el MISMO médico en consultorio 102
        var command = new ConfigurarHorarioAtencionCommand(
            1, 2, DayOfWeek.Monday, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));

        // Act & Assert
        await Assert.ThrowsAsync<HorarioAtencionSuperpuestoException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_HorarioSuperpuestoMismoConsultorio_LanzaHorarioAtencionSuperpuestoException()
    {
        // Arrange
        var medico1 = CrearMedico(1);
        var medico2 = CrearMedico(2);
        var consultorio = CrearConsultorio(1, "101");

        // Horario existente: Lunes 8 a 12 para medico1 en consultorio 101
        var horarioExistente = new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico1, consultorio);
        _horarioRepository.Entities.Add(horarioExistente);

        // Intento de horario: Lunes 9 a 13 para medico2 en el MISMO consultorio 101
        var command = new ConfigurarHorarioAtencionCommand(
            2, 1, DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0));

        // Act & Assert
        await Assert.ThrowsAsync<HorarioAtencionSuperpuestoException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ErrorAlGuardar_EjecutaRollbackTransaction()
    {
        // Arrange
        CrearMedico(1);
        CrearConsultorio(1);
        _unitOfWork.ThrowOnSave = true;

        var command = new ConfigurarHorarioAtencionCommand(
            1, 1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
    }

    [Fact]
    public async Task ObtenerHorariosPorMedico_Existente_RetornaHorarios()
    {
        // Arrange
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1, "101");
        var horario = new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico, consultorio);
        _horarioRepository.Entities.Add(horario);

        var query = new ObtenerHorariosPorMedicoQuery(1);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("101", result.Value[0].NumeroConsultorio);
    }

    [Fact]
    public async Task ObtenerHorariosPorMedico_Inexistente_RetornaFalloNotFound()
    {
        // Arrange
        var query = new ObtenerHorariosPorMedicoQuery(999);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
    }

    #region Fakes

    private class FakeRepository<T> : IRepository<T> where T : class
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

    private class FakeHorarioAtencionRepository : IHorarioAtencionRepository
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

    private class FakeUnitOfWork : IUnitOfWork
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

    #endregion
}
