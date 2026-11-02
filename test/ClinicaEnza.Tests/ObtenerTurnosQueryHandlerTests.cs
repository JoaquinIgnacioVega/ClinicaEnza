using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnos;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerTurnosQueryHandlerTests
{
    [Fact]
    public async Task Handle_SinFiltros_RetornaTurnosOrdenadosPorHora()
    {
        var turnoRepository = new FakeTurnoRepository();
        var paciente = CrearPaciente(1);
        var medico = CrearMedico(2);
        var consultorio = CrearConsultorio(3);

        turnoRepository.Entities.Add(CrearTurno(
            new DateTime(2030, 1, 10, 10, 0, 0), paciente, medico, consultorio, 1));
        turnoRepository.Entities.Add(CrearTurno(
            new DateTime(2030, 1, 10, 9, 0, 0), paciente, medico, consultorio, 2));

        var handler = new ObtenerTurnosQueryHandler(turnoRepository);
        var result = await handler.Handle(new ObtenerTurnosQuery(null, null, null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(new DateTime(2030, 1, 10, 9, 0, 0), result.Value[0].HoraInicio);
    }

    [Fact]
    public async Task Handle_FiltrosCombinados_RetornaSoloLosTurnosCoincidentes()
    {
        var turnoRepository = new FakeTurnoRepository();
        var paciente = CrearPaciente(1);
        var otroPaciente = CrearPaciente(4);
        var medico = CrearMedico(2);
        var otroMedico = CrearMedico(5);
        var consultorio = CrearConsultorio(3);

        var turnoCoincidente = CrearTurno(
            new DateTime(2030, 1, 10, 9, 0, 0), paciente, medico, consultorio, 1);
        var turnoOtraFecha = CrearTurno(
            new DateTime(2030, 1, 11, 9, 0, 0), paciente, medico, consultorio, 2);
        var turnoOtroPaciente = CrearTurno(
            new DateTime(2030, 1, 10, 9, 0, 0), otroPaciente, medico, consultorio, 3);
        var turnoOtroMedico = CrearTurno(
            new DateTime(2030, 1, 10, 9, 0, 0), paciente, otroMedico, consultorio, 4);
        turnoOtroMedico.CambiarEstado(EstadoTurno.Cancelado);

        turnoRepository.Entities.AddRange(new[]
        {
            turnoCoincidente,
            turnoOtraFecha,
            turnoOtroPaciente,
            turnoOtroMedico
        });

        var handler = new ObtenerTurnosQueryHandler(turnoRepository);
        var result = await handler.Handle(new ObtenerTurnosQuery(
            new DateTime(2030, 1, 10), 1, 2, EstadoTurno.Pendiente));

        Assert.True(result.IsSuccess);
        var turno = Assert.Single(result.Value);
        Assert.Equal(1, turno.IdTurno);
    }

    private static Paciente CrearPaciente(int id)
    {
        return new Paciente("Ana", "Ríos", $"112233{id}", new DateTime(1990, 1, 1), $"ana{id}@test.com", "OSDE")
        {
            IdPersona = id
        };
    }

    private static Medico CrearMedico(int id)
    {
        return new Medico("Carlos", "Pérez", $"223344{id}", new DateTime(1980, 1, 1), $"carlos{id}@test.com", Especialidad.Clinica, $"MP-{id}")
        {
            IdPersona = id
        };
    }

    private static Consultorio CrearConsultorio(int id)
    {
        return new Consultorio(id.ToString(), id, "Ala Norte");
    }

    private static Turno CrearTurno(DateTime horaInicio, Paciente paciente, Medico medico, Consultorio consultorio, int id)
    {
        return new Turno(horaInicio, paciente, medico, consultorio)
        {
            IdTurno = id
        };
    }
}
