using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPrescripcionesPorPaciente;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerPrescripcionesPorPacienteQueryHandlerTests
{
    private readonly FakePacienteRepository _pacienteRepository;
    private readonly ObtenerPrescripcionesPorPacienteQueryHandler _handler;

    public ObtenerPrescripcionesPorPacienteQueryHandlerTests()
    {
        _pacienteRepository = new FakePacienteRepository();
        _handler = new ObtenerPrescripcionesPorPacienteQueryHandler(_pacienteRepository);
    }

    private Medico CrearMedico(int id = 1, string nombre = "Roberto", string apellido = "García", string matricula = "MP-12345") =>
        new Medico(
            nombre,
            apellido,
            $"20111{id:D3}",
            new DateTime(1980, 5, 10),
            $"medico{id}@clinica.com",
            Especialidad.Clinica,
            matricula,
            new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };

    private Paciente CrearPaciente(int id = 10, int idHistoriaClinica = 100)
    {
        var paciente = new Paciente(
            "Lucía",
            "Méndez",
            "33445566",
            new DateTime(1995, 4, 20),
            "lucia@test.com",
            "OSDE",
            62m,
            1.65m)
        {
            IdPersona = id
        };
        paciente.HistoriaClinica!.IdHistoriaClinica = idHistoriaClinica;
        paciente.HistoriaClinica.IdPaciente = id;
        _pacienteRepository.Entities.Add(paciente);
        return paciente;
    }

    [Fact]
    public async Task Handle_PacienteConMultiplesPrescripciones_RetornaOrdenadasDescendenteConDatosMedico()
    {
        // Arrange
        var paciente = CrearPaciente(10, 100);
        var medico1 = CrearMedico(1, "Roberto", "García", "MP-11111");
        var medico2 = CrearMedico(2, "Laura", "Gómez", "MP-22222");

        var prescripcionAntigua = paciente.HistoriaClinica!.AgregarPrescripcion(1, "Ibuprofeno 400mg", medico1);
        prescripcionAntigua.IdPrescripcion = 1;

        var prescripcionReciente = paciente.HistoriaClinica.AgregarPrescripcion(2, "Amoxicilina 500mg", medico2);
        prescripcionReciente.IdPrescripcion = 2;

        var query = new ObtenerPrescripcionesPorPacienteQuery(10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var lista = result.Value.ToList();
        Assert.Equal(2, lista.Count);

        // Más reciente primero (IdPrescripcion 2 antes que IdPrescripcion 1)
        Assert.Equal(2, lista[0].IdPrescripcion);
        Assert.Equal("Amoxicilina 500mg", lista[0].Medicamento);
        Assert.Equal(2, lista[0].IdMedico);
        Assert.Equal(medico2.ObtenerNombreCompleto(), lista[0].NombreMedico);
        Assert.Equal("MP-22222", lista[0].MatriculaMedico);
        Assert.Equal(10, lista[0].IdPaciente);
        Assert.Equal(100, lista[0].IdHistoriaClinica);

        Assert.Equal(1, lista[1].IdPrescripcion);
        Assert.Equal("Ibuprofeno 400mg", lista[1].Medicamento);
        Assert.Equal(1, lista[1].IdMedico);
        Assert.Equal(medico1.ObtenerNombreCompleto(), lista[1].NombreMedico);
        Assert.Equal("MP-11111", lista[1].MatriculaMedico);
    }

    [Fact]
    public async Task Handle_PacienteSinPrescripciones_RetornaListaVacia()
    {
        // Arrange
        CrearPaciente(10, 100);
        var query = new ObtenerPrescripcionesPorPacienteQuery(10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_PacienteInexistente_RetornaFalloNotFound()
    {
        // Arrange
        var query = new ObtenerPrescripcionesPorPacienteQuery(999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente", result.Error);
    }

    #region Fakes

    private class FakePacienteRepository : FakeRepository<Paciente>, IPacienteRepository
    {
        public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.FirstOrDefault(p => p.Dni == dni));

        public Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.FirstOrDefault(p => p.IdPersona == idPaciente));
    }

    #endregion
}
