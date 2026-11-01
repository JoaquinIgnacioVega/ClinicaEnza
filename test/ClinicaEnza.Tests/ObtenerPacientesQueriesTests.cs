using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientePorId;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerPacientesQueriesTests
{
    private readonly FakePacienteRepository _pacienteRepository;
    private readonly ObtenerPacientesQueryHandler _obtenerPacientesHandler;
    private readonly ObtenerPacientePorIdQueryHandler _obtenerPacientePorIdHandler;

    public ObtenerPacientesQueriesTests()
    {
        _pacienteRepository = new FakePacienteRepository();
        _obtenerPacientesHandler = new ObtenerPacientesQueryHandler(_pacienteRepository);
        _obtenerPacientePorIdHandler = new ObtenerPacientePorIdQueryHandler(_pacienteRepository);
    }

    private void CargarPacientesDePrueba()
    {
        _pacienteRepository.Pacientes.AddRange(new[]
        {
            new Paciente("Carlos", "Gomez", "30111222", new DateTime(1990, 5, 10), "carlos@test.com", "OSDE", 78.5m, 1.76m) { IdPersona = 1 },
            new Paciente("Laura", "Perez", "32333444", new DateTime(1995, 3, 15), "laura@test.com", "Swiss Medical", 60m, 1.65m) { IdPersona = 2 },
            new Paciente("Martin", "Lopez", "40666777", new DateTime(2000, 11, 20), "martin@test.com", "Galeno", 82m, 1.80m) { IdPersona = 3 }
        });
    }

    [Fact]
    public async Task ObtenerPacientes_SinFiltro_RetornaTodosLosPacientesConEdadCalculada()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientesQuery();

        // Act
        var result = await _obtenerPacientesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);

        var p1 = result.Value.First(p => p.IdPaciente == 1);
        Assert.Equal("Carlos", p1.Nombre);
        Assert.Equal("Gomez", p1.Apellido);
        Assert.Equal("GOMEZ, Carlos", p1.NombreCompleto);
        Assert.Equal("30111222", p1.Dni);
        Assert.Equal("carlos@test.com", p1.Mail);
        Assert.Equal("OSDE", p1.ObraSocial);
        Assert.Equal(78.5m, p1.Peso);
        Assert.Equal(1.76m, p1.Altura);
        Assert.True(p1.Edad > 0);
    }

    [Fact]
    public async Task ObtenerPacientes_ConFiltroPorDni_RetornaPacientesQueCoinciden()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientesQuery("30111");

        // Act
        var result = await _obtenerPacientesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var paciente = Assert.Single(result.Value);
        Assert.Equal("30111222", paciente.Dni);
        Assert.Equal("Carlos", paciente.Nombre);
    }

    [Fact]
    public async Task ObtenerPacientes_ConFiltroPorNombre_RetornaPacientesQueCoinciden()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientesQuery("laura");

        // Act
        var result = await _obtenerPacientesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var paciente = Assert.Single(result.Value);
        Assert.Equal("Laura", paciente.Nombre);
        Assert.Equal("Perez", paciente.Apellido);
    }

    [Fact]
    public async Task ObtenerPacientes_ConFiltroPorApellido_RetornaPacientesQueCoinciden()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientesQuery("lopez");

        // Act
        var result = await _obtenerPacientesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var paciente = Assert.Single(result.Value);
        Assert.Equal("Martin", paciente.Nombre);
        Assert.Equal("Lopez", paciente.Apellido);
    }

    [Fact]
    public async Task ObtenerPacientes_ConFiltroSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientesQuery("inexistente");

        // Act
        var result = await _obtenerPacientesHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task ObtenerPacientePorId_Existente_RetornaPacienteDtoConEdadCalculada()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientePorIdQuery(2);

        // Act
        var result = await _obtenerPacientePorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.IdPaciente);
        Assert.Equal("Laura", result.Value.Nombre);
        Assert.Equal("Perez", result.Value.Apellido);
        Assert.Equal("32333444", result.Value.Dni);
        Assert.Equal("laura@test.com", result.Value.Mail);
        Assert.Equal("Swiss Medical", result.Value.ObraSocial);
        Assert.Equal(60m, result.Value.Peso);
        Assert.Equal(1.65m, result.Value.Altura);
        Assert.True(result.Value.Edad > 0);
    }

    [Fact]
    public async Task ObtenerPacientePorId_Inexistente_RetornaFalloNotFound()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientePorIdQuery(999);

        // Act
        var result = await _obtenerPacientePorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente con ID 999", result.Error);
    }

    [Fact]
    public async Task ObtenerPacientePorId_IdInvalido_RetornaFallo()
    {
        // Arrange
        CargarPacientesDePrueba();
        var query = new ObtenerPacientePorIdQuery(0);

        // Act
        var result = await _obtenerPacientePorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("mayor a cero", result.Error);
    }

    #region Fakes

    private class FakePacienteRepository : IPacienteRepository
    {
        public List<Paciente> Pacientes { get; } = new();

        public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Pacientes.FirstOrDefault(p => p.IdPersona == id));

        public Task<IReadOnlyList<Paciente>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Paciente>>(Pacientes.ToList());

        public Task<IReadOnlyList<Paciente>> FindAsync(Expression<Func<Paciente, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Paciente>>(Pacientes.AsQueryable().Where(predicate).ToList());

        public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Pacientes.FirstOrDefault(p => string.Equals(p.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default) =>
            Task.FromResult(Pacientes.FirstOrDefault(p => p.IdPersona == idPaciente));

        public Task AddAsync(Paciente entity, CancellationToken cancellationToken = default)
        {
            Pacientes.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Paciente entity) { }
        public void Delete(Paciente entity) => Pacientes.Remove(entity);
    }

    #endregion
}

