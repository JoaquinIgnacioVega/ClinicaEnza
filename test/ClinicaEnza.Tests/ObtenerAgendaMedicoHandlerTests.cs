using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerAgendaMedico;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerAgendaMedicoHandlerTests
{
    private static Medico CrearMedico(int id, string nombre = "Carlos", string apellido = "Pérez") =>
        new Medico(nombre, apellido, "22334455", DateTime.Today.AddYears(-40), "carlos@test.com", Especialidad.Clinica, $"MP-{id}00")
        {
            IdPersona = id
        };

    private static Paciente CrearPaciente(int id, string nombre = "Ana", string apellido = "Ríos", string dni = "11223344") =>
        new Paciente(nombre, apellido, dni, DateTime.Today.AddYears(-30), "ana@test.com", "OSDE")
        {
            IdPersona = id
        };

    private static Consultorio CrearConsultorio(int id = 1) =>
        new Consultorio("101", 2, "Ala Norte")
        {
            IdConsultorio = id
        };

    [Fact]
    public async Task ObtenerAgendaMedico_MedicoSolicitanteDistinto_RetornaFailureSinAutorizacion()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var medicoRepo = new FakeRepository<Medico>();
        medicoRepo.Entities.Add(CrearMedico(1));
        var handler = new ObtenerAgendaMedicoHandler(turnoRepo, medicoRepo);

        // Act
        var result = await handler.Handle(new ObtenerAgendaMedicoQuery(1, 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("No tienes autorización para consultar la agenda de otro profesional.", result.Error);
    }

    [Fact]
    public async Task ObtenerAgendaMedico_MedicoInexistente_RetornaFailureMedicoNoEncontrado()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var medicoRepo = new FakeRepository<Medico>();
        var handler = new ObtenerAgendaMedicoHandler(turnoRepo, medicoRepo);

        // Act
        var result = await handler.Handle(new ObtenerAgendaMedicoQuery(999, 999), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Médico no encontrado.", result.Error);
    }

    [Fact]
    public async Task ObtenerAgendaMedico_SinFiltros_RetornaTurnosOrdenadosCronologicamenteConDtoCompleto()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var medicoRepo = new FakeRepository<Medico>();
        var medico = CrearMedico(1);
        var otroMedico = CrearMedico(2, "Laura", "Gómez");
        var paciente = CrearPaciente(10);
        var consultorio = CrearConsultorio(3);
        medicoRepo.Entities.AddRange(new[] { medico, otroMedico });

        var fechaBase = DateTime.Today.AddDays(1);
        var turnoTarde = new Turno(fechaBase.AddHours(14), paciente, medico, consultorio) { IdTurno = 1 };
        var turnoTemprano = new Turno(fechaBase.AddHours(9), paciente, medico, consultorio) { IdTurno = 2 };
        var turnoOtroMedico = new Turno(fechaBase.AddHours(10), paciente, otroMedico, consultorio) { IdTurno = 3 };

        turnoRepo.Entities.AddRange(new[] { turnoTarde, turnoTemprano, turnoOtroMedico });
        var handler = new ObtenerAgendaMedicoHandler(turnoRepo, medicoRepo);

        // Act
        var result = await handler.Handle(new ObtenerAgendaMedicoQuery(1, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var agenda = result.Value.ToList();
        Assert.Equal(2, agenda.Count);
        Assert.Equal(2, agenda[0].IdTurno); // 09:00 primero
        Assert.Equal(1, agenda[1].IdTurno); // 14:00 segundo

        var item = agenda[0];
        Assert.Equal(10, item.IdPaciente);
        Assert.Equal("RÍOS, Ana", item.NombreCompletoPaciente);
        Assert.Equal("11223344", item.DniPaciente);
        Assert.Equal("OSDE", item.ObraSocialPaciente);
        Assert.Equal(30, item.EdadPaciente);
        Assert.Equal(3, item.IdConsultorio);
        Assert.Equal("Consultorio N° 101 (Piso 2, Sector Ala Norte)", item.UbicacionConsultorio);
        Assert.Equal("Pendiente", item.EstadoDescripcion);
    }

    [Fact]
    public async Task ObtenerAgendaMedico_ConFiltroFechaYEstado_RetornaSoloTurnosCoincidentes()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var medicoRepo = new FakeRepository<Medico>();
        var medico = CrearMedico(1);
        var paciente = CrearPaciente(10);
        var consultorio = CrearConsultorio(1);
        medicoRepo.Entities.Add(medico);

        var fechaObjetivo = DateTime.Today.AddDays(3);
        var turnoCoincidente = new Turno(fechaObjetivo.AddHours(10), paciente, medico, consultorio) { IdTurno = 1 };
        var turnoCanceladoMismaFecha = new Turno(fechaObjetivo.AddHours(11), paciente, medico, consultorio) { IdTurno = 2 };
        turnoCanceladoMismaFecha.CambiarEstado(EstadoTurno.Cancelado);
        var turnoOtraFecha = new Turno(fechaObjetivo.AddDays(1).AddHours(10), paciente, medico, consultorio) { IdTurno = 3 };

        turnoRepo.Entities.AddRange(new[] { turnoCoincidente, turnoCanceladoMismaFecha, turnoOtraFecha });
        var handler = new ObtenerAgendaMedicoHandler(turnoRepo, medicoRepo);

        // Act
        var result = await handler.Handle(
            new ObtenerAgendaMedicoQuery(1, 1, fechaObjetivo, EstadoTurno.Pendiente),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var agenda = result.Value.ToList();
        var turno = Assert.Single(agenda);
        Assert.Equal(1, turno.IdTurno);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
    }

    [Fact]
    public async Task ObtenerAgendaMedico_SinTurnosCoincidentes_RetornaListaVaciaConSuccess()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var medicoRepo = new FakeRepository<Medico>();
        medicoRepo.Entities.Add(CrearMedico(1));
        var handler = new ObtenerAgendaMedicoHandler(turnoRepo, medicoRepo);

        // Act
        var result = await handler.Handle(
            new ObtenerAgendaMedicoQuery(1, 1, DateTime.Today.AddDays(5)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}

