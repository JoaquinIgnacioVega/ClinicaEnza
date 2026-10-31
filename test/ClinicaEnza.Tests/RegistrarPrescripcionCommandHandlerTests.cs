using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class RegistrarPrescripcionCommandHandlerTests
{
    private readonly FakePacienteRepository _pacienteRepository;
    private readonly FakeMedicoRepository _medicoRepository;
    private readonly FakeRepository<PrescripcionMedica> _prescripcionRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly RegistrarPrescripcionCommandHandler _handler;

    public RegistrarPrescripcionCommandHandlerTests()
    {
        _pacienteRepository = new FakePacienteRepository();
        _medicoRepository = new FakeMedicoRepository();
        _prescripcionRepository = new FakeRepository<PrescripcionMedica>();
        _unitOfWork = new FakeUnitOfWork();

        _handler = new RegistrarPrescripcionCommandHandler(
            _pacienteRepository,
            _medicoRepository,
            _prescripcionRepository,
            _unitOfWork);
    }

    private Medico CrearMedico(int id = 1, string matricula = "MP-12345")
    {
        var medico = new Medico(
            "Roberto",
            "García",
            "20111222",
            new DateTime(1980, 5, 10),
            "roberto@clinica.com",
            Especialidad.Clinica,
            matricula,
            new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };
        _medicoRepository.Entities.Add(medico);
        return medico;
    }

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
    public async Task Handle_DatosValidos_RegistraPrescripcionYRetornaDto()
    {
        // Arrange
        var medico = CrearMedico(1, "MP-55555");
        var paciente = CrearPaciente(10, 100);
        var command = new RegistrarPrescripcionCommand(10, 1, "Enalapril 10mg cada 12hs");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10, result.Value.IdPaciente);
        Assert.Equal(100, result.Value.IdHistoriaClinica);
        Assert.Equal(1, result.Value.IdMedico);
        Assert.Equal("Enalapril 10mg cada 12hs", result.Value.Medicamento);
        Assert.Equal(medico.ObtenerNombreCompleto(), result.Value.NombreMedico);
        Assert.Equal("MP-55555", result.Value.MatriculaMedico);
        Assert.Equal(DateTime.Today, result.Value.Fecha.Date);

        Assert.Single(paciente.HistoriaClinica!.Prescripciones);
        Assert.Single(_prescripcionRepository.Entities);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_MedicamentoVacio_RetornaFallo(string medicamentoInvalido)
    {
        // Arrange
        CrearMedico(1);
        CrearPaciente(10);
        var command = new RegistrarPrescripcionCommand(10, 1, medicamentoInvalido);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("medicamento", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MedicoInexistente_RetornaFalloNotFound()
    {
        // Arrange
        CrearPaciente(10);
        var command = new RegistrarPrescripcionCommand(10, 999, "Ibuprofeno 600mg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_PacienteInexistente_RetornaFalloNotFound()
    {
        // Arrange
        CrearMedico(1);
        var command = new RegistrarPrescripcionCommand(999, 1, "Ibuprofeno 600mg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ErrorAlGuardar_EjecutaRollbackTransaction()
    {
        // Arrange
        CrearMedico(1);
        CrearPaciente(10);
        _unitOfWork.ThrowOnSave = true;
        var command = new RegistrarPrescripcionCommand(10, 1, "Paracetamol 1g");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
    }

    #region Fakes

    private class FakePacienteRepository : FakeRepository<Paciente>, IPacienteRepository
    {
        public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.FirstOrDefault(p => p.Dni == dni));

        public Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.FirstOrDefault(p => p.IdPersona == idPaciente));
    }

    private class FakeMedicoRepository : FakeRepository<Medico>, IMedicoRepository
    {
        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.Any(m => m.Dni == dni));

        public Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.Any(m => m.Matricula == matricula));

        public Task<bool> ExisteMatriculaAsync(string matricula, int? idMedicoExcluir, CancellationToken cancellationToken = default) =>
            Task.FromResult(Entities.Any(m => m.Matricula == matricula && (!idMedicoExcluir.HasValue || m.IdPersona != idMedicoExcluir.Value)));
    }

    #endregion
}
