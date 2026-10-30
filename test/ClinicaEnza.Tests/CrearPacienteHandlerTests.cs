using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests;

public class CrearPacienteHandlerTests
{
    private readonly FakePacienteRepository _pacienteRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly CrearPacienteHandler _handler;

    public CrearPacienteHandlerTests()
    {
        _pacienteRepository = new FakePacienteRepository();
        _unitOfWork = new FakeUnitOfWork();
        _handler = new CrearPacienteHandler(_pacienteRepository, _unitOfWork);
    }

    private CrearPacienteCommand CrearComandoValido()
    {
        return new CrearPacienteCommand
        {
            Nombre = "Carlos",
            Apellido = "Gutiérrez",
            Dni = "32111222",
            FechaNacimiento = new DateTime(1987, 8, 20),
            Mail = "carlos.gutierrez@test.com",
            ObraSocial = "OSDE",
            Peso = 78.5m,
            Altura = 1.76m
        };
    }

    [Fact]
    public async Task Handle_DatosValidos_RegistraPacienteConHistoriaClinicaYConfirmaTransaccion()
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
        Assert.Equal(command.Dni, result.Value.Dni);
        Assert.Equal(command.FechaNacimiento, result.Value.FechaNacimiento);
        Assert.Equal(command.Mail, result.Value.Mail);
        Assert.Equal(command.ObraSocial, result.Value.ObraSocial);
        Assert.Equal(command.Peso, result.Value.Peso);
        Assert.Equal(command.Altura, result.Value.Altura);

        Assert.Single(_pacienteRepository.Pacientes);
        var pacienteGuardado = _pacienteRepository.Pacientes[0];
        Assert.Equal(command.Dni, pacienteGuardado.Dni);
        Assert.NotNull(pacienteGuardado.HistoriaClinica);
        Assert.Same(pacienteGuardado, pacienteGuardado.HistoriaClinica.Paciente);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
        Assert.Equal(0, _unitOfWork.RollbackCallCount);
    }

    [Fact]
    public async Task Handle_DniDuplicado_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        var pacienteExistente = new Paciente(
            "Otro",
            "Paciente",
            command.Dni,
            new DateTime(1990, 1, 1),
            "otro@test.com",
            "Swiss Medical",
            70m,
            1.70m);
        _pacienteRepository.Pacientes.Add(pacienteExistente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("DNI", result.Error);
        Assert.Single(_pacienteRepository.Pacientes); // Solo el que ya existía
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, _unitOfWork.CommitCallCount);
    }

    [Theory]
    [InlineData("", "Gutiérrez", "32111222", "carlos@test.com", "OSDE")]
    [InlineData("   ", "Gutiérrez", "32111222", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "", "32111222", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "   ", "32111222", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "   ", "carlos@test.com", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "32111222", "", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "32111222", "   ", "OSDE")]
    [InlineData("Carlos", "Gutiérrez", "32111222", "carlos@test.com", "")]
    [InlineData("Carlos", "Gutiérrez", "32111222", "carlos@test.com", "   ")]
    public async Task Handle_CamposObligatoriosVacios_RetornaFallo(
        string nombre, string apellido, string dni, string mail, string obraSocial)
    {
        // Arrange
        var command = new CrearPacienteCommand
        {
            Nombre = nombre,
            Apellido = apellido,
            Dni = dni,
            FechaNacimiento = new DateTime(1987, 8, 20),
            Mail = mail,
            ObraSocial = obraSocial,
            Peso = 75m,
            Altura = 1.75m
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(_pacienteRepository.Pacientes);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_FechaNacimientoDefault_RetornaFallo()
    {
        // Arrange
        var command = CrearComandoValido();
        command.FechaNacimiento = default;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("fecha de nacimiento", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_pacienteRepository.Pacientes);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(0, 1.75)]
    [InlineData(-10, 1.75)]
    [InlineData(75, 0)]
    [InlineData(75, -1.80)]
    public async Task Handle_PesoOAlturaNoPositivos_RetornaFallo(decimal peso, decimal altura)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Peso = peso;
        command.Altura = altura;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(_pacienteRepository.Pacientes);
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

        public void Update(Paciente entity) { }
        public void Delete(Paciente entity) => Pacientes.Remove(entity);
    }

    #endregion
}

