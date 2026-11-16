using System;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.ReagendarTurno;
using ClinicaEnza.Domain;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class CrearTurnoTests
{
    [Fact]
    public void CrearTurno_ConIdsForaneos_AsignaIdsCorrectamenteYDuracion20Minutos()
    {
        var horaInicio = DateTime.Now.AddDays(1);
        int idPaciente = 5;
        int idMedico = 3;
        int idConsultorio = 2;

        var turno = new Turno(horaInicio, idPaciente, idMedico, idConsultorio);

        Assert.Equal(horaInicio, turno.HoraInicio);
        Assert.Equal(horaInicio.AddMinutes(20), turno.HoraFin);
        Assert.Equal(idPaciente, turno.IdPaciente);
        Assert.Equal(idMedico, turno.IdMedico);
        Assert.Equal(idConsultorio, turno.IdConsultorio);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
    }

    [Fact]
    public void CrearTurno_FechaPasada_LanzaArgumentException()
    {
        var fechaPasada = DateTime.Now.AddMinutes(-5);

        Assert.Throws<ArgumentException>(() => new Turno(fechaPasada, 1, 1, 1));
    }

    [Fact]
    public async Task CrearTurnoHandler_ConsultorioInactivo_RetornaFalloSinGuardar()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var pacienteRepo = new FakeRepository<Paciente>();
        var medicoRepo = new FakeRepository<Medico>();
        var horarioRepoBase = new FakeRepository<HorarioAtencion>();
        var horarioRepo = new FakeHorarioAtencionRepository(horarioRepoBase);
        var unitOfWork = new FakeUnitOfWork();

        var paciente = new Paciente("Ana", "Ríos", "11223344", DateTime.Today.AddYears(-28), "ana@test.com", "OSDE")
        {
            IdPersona = 1
        };
        var medico = new Medico("Carlos", "Pérez", "22334455", DateTime.Today.AddYears(-40), "carlos@test.com", Especialidad.Clinica, "MP-100")
        {
            IdPersona = 2
        };
        var consultorio = new Consultorio("101", 1, "Ala Norte")
        {
            IdConsultorio = 3
        };
        consultorio.Desactivar();

        var fechaTurno = DateTime.Today.AddDays(1).AddHours(10);
        var horario = new HorarioAtencion(fechaTurno.DayOfWeek, new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), medico, consultorio)
        {
            IdHorarioAtencion = 1
        };

        pacienteRepo.Entities.Add(paciente);
        medicoRepo.Entities.Add(medico);
        horarioRepoBase.Entities.Add(horario);

        var handler = new CrearTurnoCommandHandler(turnoRepo, pacienteRepo, medicoRepo, horarioRepo, unitOfWork);

        // Act
        var result = await handler.Handle(new CrearTurnoCommand(1, 2, fechaTurno), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("inactivo", result.Error);
        Assert.Empty(turnoRepo.Entities);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ReagendarTurnoHandler_ConsultorioInactivo_RetornaFalloSinGuardar()
    {
        // Arrange
        var turnoRepo = new FakeTurnoRepository();
        var horarioRepoBase = new FakeRepository<HorarioAtencion>();
        var horarioRepo = new FakeHorarioAtencionRepository(horarioRepoBase);
        var unitOfWork = new FakeUnitOfWork();

        var medico = new Medico("Carlos", "Pérez", "22334455", DateTime.Today.AddYears(-40), "carlos@test.com", Especialidad.Clinica, "MP-100")
        {
            IdPersona = 2
        };
        var consultorioActivo = new Consultorio("101", 1, "Ala Norte") { IdConsultorio = 3 };
        var consultorioInactivo = new Consultorio("102", 1, "Ala Sur") { IdConsultorio = 4 };
        consultorioInactivo.Desactivar();

        var horaOriginal = DateTime.Today.AddDays(1).AddHours(10);
        var nuevaHora = DateTime.Today.AddDays(2).AddHours(11);

        var turno = new Turno(horaOriginal, 1, 2, 3) { IdTurno = 10 };
        turnoRepo.Entities.Add(turno);

        var horarioDestino = new HorarioAtencion(nuevaHora.DayOfWeek, new TimeSpan(8, 0, 0), new TimeSpan(14, 0, 0), medico, consultorioInactivo)
        {
            IdHorarioAtencion = 2
        };
        horarioRepoBase.Entities.Add(horarioDestino);

        var handler = new ReagendarTurnoCommandHandler(turnoRepo, horarioRepo, unitOfWork);

        // Act
        var result = await handler.Handle(new ReagendarTurnoCommand(10, nuevaHora), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("inactivo", result.Error);
        Assert.Equal(horaOriginal, turno.HoraInicio);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
