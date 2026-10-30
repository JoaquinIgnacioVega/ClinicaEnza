using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosDisponibles;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerHorariosDisponiblesQueryHandlerTests
{
    private readonly FakeRepository<HorarioAtencion> _baseHorarioRepo;
    private readonly FakeHorarioAtencionRepository _horarioRepository;
    private readonly FakeTurnoRepository _turnoRepository;
    private readonly ObtenerHorariosDisponiblesQueryHandler _handler;

    public ObtenerHorariosDisponiblesQueryHandlerTests()
    {
        _baseHorarioRepo = new FakeRepository<HorarioAtencion>();
        _horarioRepository = new FakeHorarioAtencionRepository(_baseHorarioRepo);
        _turnoRepository = new FakeTurnoRepository();
        _handler = new ObtenerHorariosDisponiblesQueryHandler(_horarioRepository, _turnoRepository);
    }

    private Medico CrearMedico(int id = 1) =>
        new Medico("Carlos", "Gomez", "20111222", new DateTime(1980, 5, 10), "carlos@test.com", Especialidad.Cardiologia, "MP-100", new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };

    private Paciente CrearPaciente(int id = 10) =>
        new Paciente("Ana", "Lopez", "30111222", new DateTime(1990, 3, 15), "ana@test.com", "OSDE")
        {
            IdPersona = id
        };

    private Consultorio CrearConsultorio(int id = 1, string numero = "101", int piso = 1, string sector = "Ala Norte") =>
        new Consultorio(numero, piso, sector)
        {
            IdConsultorio = id
        };

    private static DateTime ObtenerProximoDiaSemana(DayOfWeek diaSemana)
    {
        var fecha = DateTime.Today.AddDays(1);
        while (fecha.DayOfWeek != diaSemana)
        {
            fecha = fecha.AddDays(1);
        }
        return fecha;
    }

    [Fact]
    public async Task Handle_MedicoNoAtiendeEseDia_RetornaListaVacia()
    {
        // Arrange: Médico atiende solo Lunes, consultamos un Martes futuro
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1, "101");
        _baseHorarioRepo.Entities.Add(new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), medico, consultorio));

        var proximoMartes = ObtenerProximoDiaSemana(DayOfWeek.Tuesday);
        var query = new ObtenerHorariosDisponiblesQuery(1, proximoMartes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_MedicoConHorarioSinTurnos_RetornaTodosLosSlotsDe20Minutos()
    {
        // Arrange: Lunes de 08:00 a 09:00 -> 3 bloques de 20 min (08:00, 08:20, 08:40)
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1, "101", 1, "Ala Norte");
        _baseHorarioRepo.Entities.Add(new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), medico, consultorio));

        var proximoLunes = ObtenerProximoDiaSemana(DayOfWeek.Monday);
        var query = new ObtenerHorariosDisponiblesQuery(1, proximoLunes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var slots = result.Value.ToList();
        Assert.Equal(3, slots.Count);

        Assert.Equal(proximoLunes.AddHours(8), slots[0].HoraInicio);
        Assert.Equal(proximoLunes.AddHours(8).AddMinutes(20), slots[0].HoraFin);
        Assert.Equal(1, slots[0].IdConsultorio);
        Assert.Equal(consultorio.ObtenerUbicacion(), slots[0].UbicacionConsultorio);

        Assert.Equal(proximoLunes.AddHours(8).AddMinutes(20), slots[1].HoraInicio);
        Assert.Equal(proximoLunes.AddHours(8).AddMinutes(40), slots[1].HoraFin);

        Assert.Equal(proximoLunes.AddHours(8).AddMinutes(40), slots[2].HoraInicio);
        Assert.Equal(proximoLunes.AddHours(9), slots[2].HoraFin);
    }

    [Fact]
    public async Task Handle_MedicoConTurnoActivo_DescartaSlotOcupado()
    {
        // Arrange: Lunes de 08:00 a 09:00 con un turno activo a las 08:20
        var medico = CrearMedico(1);
        var paciente = CrearPaciente(10);
        var consultorio = CrearConsultorio(1, "101");
        _baseHorarioRepo.Entities.Add(new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), medico, consultorio));

        var proximoLunes = ObtenerProximoDiaSemana(DayOfWeek.Monday);
        var turnoActivo = new Turno(proximoLunes.AddHours(8).AddMinutes(20), paciente, medico, consultorio);
        _turnoRepository.Entities.Add(turnoActivo);

        var query = new ObtenerHorariosDisponiblesQuery(1, proximoLunes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var slots = result.Value.ToList();
        Assert.Equal(2, slots.Count);
        Assert.DoesNotContain(slots, s => s.HoraInicio == proximoLunes.AddHours(8).AddMinutes(20));
        Assert.Equal(proximoLunes.AddHours(8), slots[0].HoraInicio);
        Assert.Equal(proximoLunes.AddHours(8).AddMinutes(40), slots[1].HoraInicio);
    }

    [Fact]
    public async Task Handle_MedicoConTurnoCancelado_MantieneSlotDisponible()
    {
        // Arrange: Lunes de 08:00 a 09:00 con un turno CANCELADO a las 08:20
        var medico = CrearMedico(1);
        var paciente = CrearPaciente(10);
        var consultorio = CrearConsultorio(1, "101");
        _baseHorarioRepo.Entities.Add(new HorarioAtencion(
            DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0), medico, consultorio));

        var proximoLunes = ObtenerProximoDiaSemana(DayOfWeek.Monday);
        var turnoCancelado = new Turno(proximoLunes.AddHours(8).AddMinutes(20), paciente, medico, consultorio);
        turnoCancelado.CambiarEstado(EstadoTurno.Cancelado);
        _turnoRepository.Entities.Add(turnoCancelado);

        var query = new ObtenerHorariosDisponiblesQuery(1, proximoLunes);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var slots = result.Value.ToList();
        Assert.Equal(3, slots.Count);
        Assert.Contains(slots, s => s.HoraInicio == proximoLunes.AddHours(8).AddMinutes(20));
    }

    [Fact]
    public async Task Handle_FechaEnElPasado_DescartaTodosLosSlotsPasados()
    {
        // Arrange: Fecha de la semana pasada
        var medico = CrearMedico(1);
        var consultorio = CrearConsultorio(1, "101");
        var fechaPasada = DateTime.Today.AddDays(-7);

        _baseHorarioRepo.Entities.Add(new HorarioAtencion(
            fechaPasada.DayOfWeek, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), medico, consultorio));

        var query = new ObtenerHorariosDisponiblesQuery(1, fechaPasada);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
