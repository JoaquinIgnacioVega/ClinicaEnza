using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.Commands.ModificarMedico;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class ModificarMedicoCommandHandlerTests
{
    private readonly FakeMedicoUnitOfWork _unitOfWork;
    private readonly ModificarMedicoCommandHandler _handler;

    public ModificarMedicoCommandHandlerTests()
    {
        _unitOfWork = new FakeMedicoUnitOfWork();
        _handler = new ModificarMedicoCommandHandler(_unitOfWork);
    }

    private Medico CrearMedicoExistente(int id = 1, string matricula = "MP-1000")
    {
        var medico = new Medico(
            "Carlos",
            "Perez",
            "20111222",
            new DateTime(1980, 5, 10),
            "carlos.original@clinicaenza.com",
            Especialidad.Clinica,
            matricula,
            new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };

        _unitOfWork.MedicoRepository.Medicos.Add(medico);
        return medico;
    }

    [Fact]
    public async Task Handle_DatosValidos_ModificaMedicoYConfirmaTransaccion()
    {
        // Arrange
        var medico = CrearMedicoExistente(1, "MP-1000");
        var command = new ModificarMedicoCommand(
            1,
            "Carlos Alberto",
            "Perez Gomez",
            "carlos.nuevo@clinicaenza.com",
            Especialidad.Cardiologia,
            "MP-1000-B");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Carlos Alberto", result.Value.Nombre);
        Assert.Equal("Perez Gomez", result.Value.Apellido);
        Assert.Equal("carlos.nuevo@clinicaenza.com", result.Value.Mail);
        Assert.Equal(Especialidad.Cardiologia, result.Value.Especialidad);
        Assert.Equal("MP-1000-B", result.Value.Matricula);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
        Assert.Equal(0, _unitOfWork.RollbackCallCount);
    }

    [Fact]
    public async Task Handle_MismaMatriculaMismoMedico_ModificaExitosamente()
    {
        // Arrange
        var medico = CrearMedicoExistente(1, "MP-1000");
        var command = new ModificarMedicoCommand(
            1,
            "Carlos Actualizado",
            "Perez",
            "carlos.mod@clinicaenza.com",
            Especialidad.Dermatologia,
            "MP-1000"); // Misma matrícula del mismo médico

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("MP-1000", result.Value.Matricula);
        Assert.Equal("Carlos Actualizado", result.Value.Nombre);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_MedicoNoExiste_RetornaFalloNotFound()
    {
        // Arrange
        var command = new ModificarMedicoCommand(
            999,
            "Inexistente",
            "Medico",
            "inexistente@clinicaenza.com",
            Especialidad.Pediatria,
            "MP-9999");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MatriculaDuplicadaEnOtroMedico_RetornaFallo()
    {
        // Arrange
        var medico1 = CrearMedicoExistente(1, "MP-1000");
        var medico2 = CrearMedicoExistente(2, "MP-2000");

        // Intentar actualizar medico1 con la matrícula de medico2
        var command = new ModificarMedicoCommand(
            1,
            "Carlos",
            "Perez",
            "carlos@clinicaenza.com",
            Especialidad.Clinica,
            "MP-2000");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("matrícula", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(0, "Carlos", "Perez", "carlos@test.com", "MP-100")]
    [InlineData(-1, "Carlos", "Perez", "carlos@test.com", "MP-100")]
    [InlineData(1, "", "Perez", "carlos@test.com", "MP-100")]
    [InlineData(1, "Carlos", "", "carlos@test.com", "MP-100")]
    [InlineData(1, "Carlos", "Perez", "", "MP-100")]
    [InlineData(1, "Carlos", "Perez", "carlos@test.com", "")]
    public async Task Handle_CamposObligatoriosVaciosOInvalidos_RetornaFallo(
        int id, string nombre, string apellido, string mail, string matricula)
    {
        // Arrange
        CrearMedicoExistente(1, "MP-ORIG");
        var command = new ModificarMedicoCommand(id, nombre, apellido, mail, Especialidad.Clinica, matricula);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_EspecialidadInvalida_RetornaFallo()
    {
        // Arrange
        CrearMedicoExistente(1, "MP-ORIG");
        var command = new ModificarMedicoCommand(
            1, "Carlos", "Perez", "carlos@test.com", (Especialidad)999, "MP-NUEVA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("especialidad", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ErrorAlGuardar_EjecutaRollbackTransaction()
    {
        // Arrange
        CrearMedicoExistente(1, "MP-1000");
        _unitOfWork.ThrowOnSave = true;

        var command = new ModificarMedicoCommand(
            1, "Carlos", "Perez", "carlos@test.com", Especialidad.Clinica, "MP-1000");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
    }

    [Fact]
    public void ActualizarInformacion_DatosValidos_ActualizaPropiedades()
    {
        // Arrange
        var medico = new Medico("Juan", "Lopez", "12345678", new DateTime(1985, 1, 1), "juan@test.com", Especialidad.Clinica, "MP-1", new DateTime(2020, 1, 1));

        // Act
        medico.ActualizarInformacion("Juan Carlos", "Lopez Gomez", "juancarlos@test.com", Especialidad.Pediatria, "MP-2");

        // Assert
        Assert.Equal("Juan Carlos", medico.Nombre);
        Assert.Equal("Lopez Gomez", medico.Apellido);
        Assert.Equal("juancarlos@test.com", medico.Mail);
        Assert.Equal(Especialidad.Pediatria, medico.Especialidad);
        Assert.Equal("MP-2", medico.Matricula);
    }

    #region Fakes

    private class FakeMedicoUnitOfWork : IUnitOfWork
    {
        public FakeInnerMedicoRepository MedicoRepository { get; } = new();

        public IUsuarioRepository Usuarios => throw new NotImplementedException();
        public IMedicoRepository Medicos => MedicoRepository;

        public int SaveChangesCallCount { get; private set; }
        public int CommitCallCount { get; private set; }
        public int RollbackCallCount { get; private set; }
        public bool ThrowOnSave { get; set; }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

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

    private class FakeInnerMedicoRepository : IMedicoRepository
    {
        public List<Medico> Medicos { get; } = new();

        public Task AddAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            Medicos.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default) =>
            ExisteMatriculaAsync(matricula, null, cancellationToken);

        public Task<bool> ExisteMatriculaAsync(string matricula, int? idMedicoExcluir, CancellationToken cancellationToken = default)
        {
            var existe = Medicos.Any(m =>
                string.Equals(m.Matricula, matricula, StringComparison.OrdinalIgnoreCase)
                && (!idMedicoExcluir.HasValue || m.IdPersona != idMedicoExcluir.Value));
            return Task.FromResult(existe);
        }

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            var existe = Medicos.Any(m => string.Equals(m.Dni, dni, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

        public Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.FirstOrDefault(m => m.IdPersona == id));

        public Task<IReadOnlyList<Medico>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.ToList());

        public Task<IReadOnlyList<Medico>> FindAsync(Expression<Func<Medico, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.AsQueryable().Where(predicate).ToList());

        public void Update(Medico entity) { }
        public void Delete(Medico entity) => Medicos.Remove(entity);
    }

    #endregion
}
