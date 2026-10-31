using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Commands.ModificarPaciente;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests;

public class ModificarPacienteHandlerTests
{
    private readonly FakePacienteRepository _pacienteRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly ModificarPacienteHandler _handler;

    public ModificarPacienteHandlerTests()
    {
        _pacienteRepository = new FakePacienteRepository();
        _unitOfWork = new FakeUnitOfWork();
        _handler = new ModificarPacienteHandler(_pacienteRepository, _unitOfWork);
    }

    private Paciente CrearPacienteExistente(int id = 1)
    {
        var paciente = new Paciente(
            "Carlos",
            "Gutiérrez",
            "32111222",
            new DateTime(1987, 8, 20),
            "carlos.original@test.com",
            "OSDE",
            75m,
            1.75m)
        {
            IdPersona = id
        };

        _pacienteRepository.Pacientes.Add(paciente);
        return paciente;
    }

    [Fact]
    public async Task Handle_DatosValidos_ModificaPacienteYConfirmaTransaccion()
    {
        // Arrange
        var paciente = CrearPacienteExistente(1);
        var command = new ModificarPacienteCommand(
            idPaciente: 1,
            nombre: "Carlos Alberto",
            apellido: "Gutiérrez Gómez",
            mail: "carlos.nuevo@test.com",
            obraSocial: "Swiss Medical",
            peso: 80.5m,
            altura: 1.77m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.IdPaciente);
        Assert.Equal("Carlos Alberto", result.Value.Nombre);
        Assert.Equal("Gutiérrez Gómez", result.Value.Apellido);
        Assert.Equal("carlos.nuevo@test.com", result.Value.Mail);
        Assert.Equal("Swiss Medical", result.Value.ObraSocial);
        Assert.Equal(80.5m, result.Value.Peso);
        Assert.Equal(1.77m, result.Value.Altura);

        // Verificar entidad en repositorio
        Assert.Equal("Carlos Alberto", paciente.Nombre);
        Assert.Equal("Gutiérrez Gómez", paciente.Apellido);
        Assert.Equal("carlos.nuevo@test.com", paciente.Mail);
        Assert.Equal("Swiss Medical", paciente.ObraSocial);
        Assert.Equal(80.5m, paciente.Peso);
        Assert.Equal(1.77m, paciente.Altura);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
        Assert.Equal(0, _unitOfWork.RollbackCallCount);
        Assert.Equal(1, _pacienteRepository.UpdateCallCount);
    }

    [Fact]
    public async Task Handle_PacienteNoExiste_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = new ModificarPacienteCommand(
            idPaciente: 999,
            nombre: "Carlos",
            apellido: "Gutiérrez",
            mail: "carlos@test.com",
            obraSocial: "OSDE",
            peso: 75m,
            altura: 1.75m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente con ID 999", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, _unitOfWork.CommitCallCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_IdInvalido_RetornaFalloSinGuardar(int idInvalido)
    {
        // Arrange
        var command = new ModificarPacienteCommand(
            idPaciente: idInvalido,
            nombre: "Carlos",
            apellido: "Gutiérrez",
            mail: "carlos@test.com",
            obraSocial: "OSDE",
            peso: 75m,
            altura: 1.75m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("ID del paciente debe ser válido", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("", "Gutiérrez", "carlos@test.com", "OSDE")]
    [InlineData("   ", "Gutiérrez", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "   ", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "   ", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "carlos@test.com", "")]
    [InlineData("Carlos", "Gutiérrez", "carlos@test.com", "   ")]
    public async Task Handle_CamposObligatoriosVacios_RetornaFalloSinGuardar(
        string nombre, string apellido, string mail, string obraSocial)
    {
        // Arrange
        CrearPacienteExistente(1);
        var command = new ModificarPacienteCommand(
            idPaciente: 1,
            nombre: nombre,
            apellido: apellido,
            mail: mail,
            obraSocial: obraSocial,
            peso: 75m,
            altura: 1.75m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(0, 1.75)]
    [InlineData(-10, 1.75)]
    [InlineData(75, 0)]
    [InlineData(75, -1.80)]
    public async Task Handle_PesoOAlturaNoPositivos_RetornaFalloSinGuardar(decimal peso, decimal altura)
    {
        // Arrange
        CrearPacienteExistente(1);
        var command = new ModificarPacienteCommand(
            idPaciente: 1,
            nombre: "Carlos",
            apellido: "Gutiérrez",
            mail: "carlos@test.com",
            obraSocial: "OSDE",
            peso: peso,
            altura: altura);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ErrorAlGuardar_EjecutaRollbackTransaction()
    {
        // Arrange
        CrearPacienteExistente(1);
        var command = new ModificarPacienteCommand(
            idPaciente: 1,
            nombre: "Carlos",
            apellido: "Gutiérrez",
            mail: "carlos.nuevo@test.com",
            obraSocial: "Swiss Medical",
            peso: 80m,
            altura: 1.75m);

        _unitOfWork.ThrowOnSave = true;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
    }

    #region Fakes

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }
        public int CommitCallCount { get; private set; }
        public int RollbackCallCount { get; private set; }
        public int BeginCallCount { get; private set; }
        public bool ThrowOnSave { get; set; }

        public IUsuarioRepository Usuarios => throw new NotImplementedException();
        public IMedicoRepository Medicos => throw new NotImplementedException();

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginCallCount++;
            return Task.CompletedTask;
        }

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

    private class FakePacienteRepository : IPacienteRepository
    {
        public List<Paciente> Pacientes { get; } = new();
        public int UpdateCallCount { get; private set; }

        public Task AddAsync(Paciente entity, CancellationToken cancellationToken = default)
        {
            Pacientes.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            var p = Pacientes.FirstOrDefault(x => string.Equals(x.Dni, dni, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(p);
        }

        public Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default)
        {
            var p = Pacientes.FirstOrDefault(x => x.IdPersona == idPaciente);
            return Task.FromResult(p);
        }

        public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var p = Pacientes.FirstOrDefault(x => x.IdPersona == id);
            return Task.FromResult(p);
        }

        public Task<IReadOnlyList<Paciente>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Paciente>>(Pacientes.ToList());

        public Task<IReadOnlyList<Paciente>> FindAsync(Expression<Func<Paciente, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Paciente>>(Pacientes.AsQueryable().Where(predicate).ToList());

        public void Update(Paciente entity)
        {
            UpdateCallCount++;
        }

        public void Delete(Paciente entity) => Pacientes.Remove(entity);
    }

    #endregion
}

