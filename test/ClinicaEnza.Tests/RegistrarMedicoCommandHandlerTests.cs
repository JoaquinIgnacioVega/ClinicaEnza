using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.Commands.RegistrarMedico;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class RegistrarMedicoCommandHandlerTests
{
    private readonly FakeMedicoUnitOfWork _unitOfWork;
    private readonly RegistrarMedicoCommandHandler _handler;

    public RegistrarMedicoCommandHandlerTests()
    {
        _unitOfWork = new FakeMedicoUnitOfWork();
        _handler = new RegistrarMedicoCommandHandler(_unitOfWork);
    }

    private RegistrarMedicoCommand CrearComandoValido()
    {
        return new RegistrarMedicoCommand
        {
            Nombre = "Laura",
            Apellido = "Gomez",
            Dni = "28765432",
            FechaNacimiento = new DateTime(1980, 6, 15),
            Mail = "laura@clinicaenza.com",
            Matricula = "MP-1234",
            Especialidad = Especialidad.Cardiologia,
            FechaInicio = new DateTime(2015, 3, 1)
        };
    }

    [Fact]
    public async Task Handle_DatosValidos_RegistraMedicoYConfirmaTransaccion()
    {
        // Arrange
        var command = CrearComandoValido();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Nombre, result.Value.Nombre);
        Assert.Equal(command.Apellido, result.Value.Apellido);
        Assert.Equal(command.Matricula, result.Value.Matricula);
        Assert.Equal(command.Especialidad, result.Value.Especialidad);
        Assert.Equal(command.FechaInicio, result.Value.FechaInicio);
        Assert.Single(_unitOfWork.MedicoRepository.Medicos);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_MatriculaDuplicada_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        _unitOfWork.MedicoRepository.MatriculasExistentes.Add(command.Matricula);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("matr\u00edcula", result.Error);
        Assert.Empty(_unitOfWork.MedicoRepository.Medicos);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DniDuplicado_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        _unitOfWork.MedicoRepository.DnisExistentes.Add(command.Dni);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("DNI", result.Error);
        Assert.Empty(_unitOfWork.MedicoRepository.Medicos);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("", "Gomez", "28765432", "laura@test.com", "MP-1234")]
    [InlineData("Laura", "", "28765432", "laura@test.com", "MP-1234")]
    [InlineData("Laura", "Gomez", "", "laura@test.com", "MP-1234")]
    [InlineData("Laura", "Gomez", "28765432", "", "MP-1234")]
    [InlineData("Laura", "Gomez", "28765432", "laura@test.com", "")]
    public async Task Handle_CamposObligatoriosVacios_RetornaFallo(
        string nombre, string apellido, string dni, string mail, string matricula)
    {
        // Arrange
        var command = new RegistrarMedicoCommand
        {
            Nombre = nombre,
            Apellido = apellido,
            Dni = dni,
            FechaNacimiento = new DateTime(1980, 6, 15),
            Mail = mail,
            Matricula = matricula,
            Especialidad = Especialidad.Clinica,
            FechaInicio = new DateTime(2015, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_FechaInicioDefault_RetornaFallo()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaInicio = default;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("inicio", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_EspecialidadInvalida_RetornaFallo()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Especialidad = (Especialidad)999;

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
        var command = CrearComandoValido();
        _unitOfWork.ThrowOnSave = true;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
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
        public HashSet<string> MatriculasExistentes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> DnisExistentes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task AddAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            Medicos.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default) =>
            ExisteMatriculaAsync(matricula, null, cancellationToken);

        public Task<bool> ExisteMatriculaAsync(string matricula, int? idMedicoExcluir, CancellationToken cancellationToken = default)
        {
            var existe = MatriculasExistentes.Contains(matricula)
                         || Medicos.Any(m => string.Equals(m.Matricula, matricula, StringComparison.OrdinalIgnoreCase) && (!idMedicoExcluir.HasValue || m.IdPersona != idMedicoExcluir.Value));
            return Task.FromResult(existe);
        }

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            var existe = DnisExistentes.Contains(dni)
                         || Medicos.Any(m => string.Equals(m.Dni, dni, StringComparison.OrdinalIgnoreCase));
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
