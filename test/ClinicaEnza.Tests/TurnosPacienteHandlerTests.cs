using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurnoPaciente;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorPaciente;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class TurnosPacienteHandlerTests
{
    private static Paciente CrearPaciente(int id = 1, string nombre = "Ana", string apellido = "Ríos", string dni = "11223344")
    {
        return new Paciente(nombre, apellido, dni, DateTime.Today.AddYears(-25), "ana@test.com", "OSDE")
        {
            IdPersona = id
        };
    }

    private static Medico CrearMedico(int id = 1)
    {
        return new Medico("Carlos", "Pérez", "22334455", DateTime.Today.AddYears(-40), "carlos@test.com", Especialidad.Clinica, "MP-100")
        {
            IdPersona = id
        };
    }

    private static Consultorio CrearConsultorio(int id = 1)
    {
        return new Consultorio("101", 1, "Ala Norte")
        {
            IdConsultorio = id
        };
    }

    [Fact]
    public async Task ObtenerTurnosPorPaciente_PacienteExistente_RetornaTurnosPendientesOrdenadosPorHoraInicio()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var pacienteRepo = new FakePacienteRepo();
        var paciente = CrearPaciente(10);
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1);
        pacienteRepo.Pacientes.Add(paciente);

        var fechaBase = DateTime.Now.AddDays(2);
        var turno1 = new Turno(fechaBase.AddHours(2), paciente, medico, consultorio) { IdTurno = 1 };
        var turno2 = new Turno(fechaBase.AddHours(1), paciente, medico, consultorio) { IdTurno = 2 };
        var turnoCancelado = new Turno(fechaBase.AddHours(3), paciente, medico, consultorio) { IdTurno = 3 };
        turnoCancelado.CambiarEstado(EstadoTurno.Cancelado);

        turnoRepo.Entities.AddRange(new[] { turno1, turno2, turnoCancelado });

        var handler = new ObtenerTurnosPorPacienteQueryHandler(turnoRepo, pacienteRepo);

        // Act
        var result = await handler.Handle(new ObtenerTurnosPorPacienteQuery(10, SoloPendientes: true), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(2, result.Value[0].IdTurno); // Hora anterior primero
        Assert.Equal(1, result.Value[1].IdTurno);
        Assert.All(result.Value, t => Assert.Equal(EstadoTurno.Pendiente, t.Estado));
    }

    [Fact]
    public async Task ObtenerTurnosPorPaciente_PacienteInexistente_RetornaFallo()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var pacienteRepo = new FakePacienteRepo();
        var handler = new ObtenerTurnosPorPacienteQueryHandler(turnoRepo, pacienteRepo);

        // Act
        var result = await handler.Handle(new ObtenerTurnosPorPacienteQuery(999), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente", result.Error);
    }

    [Fact]
    public async Task ObtenerTurnosPorPaciente_IdInvalido_RetornaFallo()
    {
        // Arrange
        var handler = new ObtenerTurnosPorPacienteQueryHandler(new FakeTurnoRepository(), new FakePacienteRepo());

        // Act
        var result = await handler.Handle(new ObtenerTurnosPorPacienteQuery(0), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("válido", result.Error);
    }

    [Fact]
    public async Task CancelarTurnoPaciente_TurnoExistenteYValido_CancelaExitosamente()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fechaTurno = DateTime.Now.AddDays(2);
        var fechaActual = DateTime.Now;
        var dateTimeService = new FakeDateTimeService(fechaActual);

        var paciente = CrearPaciente(5);
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1);

        var turno = new Turno(fechaTurno, paciente, medico, consultorio)
        {
            IdTurno = 100
        };
        turnoRepo.Entities.Add(turno);

        var handler = new CancelarTurnoPacienteCommandHandler(turnoRepo, unitOfWork, dateTimeService);

        // Act
        var result = await handler.Handle(new CancelarTurnoPacienteCommand(100, 5), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoTurno.Cancelado, result.Value.Estado);
        Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CancelarTurnoPaciente_TurnoInexistente_RetornaFalloExacto()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var dateTimeService = new FakeDateTimeService();

        var handler = new CancelarTurnoPacienteCommandHandler(turnoRepo, unitOfWork, dateTimeService);

        // Act
        var result = await handler.Handle(new CancelarTurnoPacienteCommand(999, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("El turno solicitado no existe.", result.Error);
    }

    [Fact]
    public async Task CancelarTurnoPaciente_PacienteDistinto_RetornaFalloPermiso()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fechaTurno = DateTime.Now.AddDays(2);
        var fechaActual = DateTime.Now;
        var dateTimeService = new FakeDateTimeService(fechaActual);

        var pacienteDueño = CrearPaciente(5);
        var turno = new Turno(fechaTurno, pacienteDueño, CrearMedico(), CrearConsultorio())
        {
            IdTurno = 100
        };
        turnoRepo.Entities.Add(turno);

        var handler = new CancelarTurnoPacienteCommandHandler(turnoRepo, unitOfWork, dateTimeService);

        // Act (Paciente 99 intenta cancelar turno del paciente 5)
        var result = await handler.Handle(new CancelarTurnoPacienteCommand(100, 99), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("No tienes permiso para cancelar un turno que pertenece a otro paciente.", result.Error);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
    }

    [Fact]
    public async Task CancelarTurnoPaciente_TurnoNoPendiente_RetornaFalloEstado()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fechaTurno = DateTime.Now.AddDays(2);
        var fechaActual = DateTime.Now;
        var dateTimeService = new FakeDateTimeService(fechaActual);

        var paciente = CrearPaciente(5);
        var turno = new Turno(fechaTurno, paciente, CrearMedico(), CrearConsultorio())
        {
            IdTurno = 100
        };
        turno.CambiarEstado(EstadoTurno.Realizado);
        turnoRepo.Entities.Add(turno);

        var handler = new CancelarTurnoPacienteCommandHandler(turnoRepo, unitOfWork, dateTimeService);

        // Act
        var result = await handler.Handle(new CancelarTurnoPacienteCommand(100, 5), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Solo se pueden cancelar turnos que se encuentren en estado pendiente.", result.Error);
    }

    [Fact]
    public async Task CancelarTurnoPaciente_TurnoVencido_RetornaFalloVencido()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fechaTurno = DateTime.Now.AddMinutes(5);
        var fechaActual = fechaTurno.AddMinutes(10); // Posterior a la fecha del turno
        var dateTimeService = new FakeDateTimeService(fechaActual);

        var paciente = CrearPaciente(5);
        var turno = new Turno(fechaTurno, paciente, CrearMedico(), CrearConsultorio())
        {
            IdTurno = 100
        };
        turnoRepo.Entities.Add(turno);

        var handler = new CancelarTurnoPacienteCommandHandler(turnoRepo, unitOfWork, dateTimeService);

        // Act
        var result = await handler.Handle(new CancelarTurnoPacienteCommand(100, 5), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("No se puede cancelar un turno cuya fecha y hora ya han vencido.", result.Error);
    }

    private class FakePacienteRepo : IPacienteRepository
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
}

