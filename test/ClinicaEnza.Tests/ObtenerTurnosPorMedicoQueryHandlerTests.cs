using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerTurnosPorMedicoQueryHandlerTests
{
    [Fact]
    public async Task Handle_MedicoExistenteConFiltros_RetornaTurnosCoincidentes()
    {
        var turnoRepository = new FakeTurnoRepository();
        var medicoRepository = new FakeRepository<Medico>();
        var medico = CrearMedico(1);
        var paciente = CrearPaciente(2);
        var consultorio = new Consultorio("101", 3, "Ala Norte");
        medicoRepository.Entities.Add(medico);

        var turnoCoincidente = new Turno(
            new DateTime(2030, 2, 10, 9, 0, 0), paciente, medico, consultorio)
        {
            IdTurno = 1
        };
        var turnoOtraFecha = new Turno(
            new DateTime(2030, 2, 11, 9, 0, 0), paciente, medico, consultorio)
        {
            IdTurno = 2
        };
        turnoOtraFecha.CambiarEstado(EstadoTurno.Cancelado);
        turnoRepository.Entities.AddRange(new[] { turnoCoincidente, turnoOtraFecha });

        var handler = new ObtenerTurnosPorMedicoQueryHandler(turnoRepository, medicoRepository);
        var result = await handler.Handle(new ObtenerTurnosPorMedicoQuery(
            1, new DateTime(2030, 2, 10), EstadoTurno.Pendiente));

        var turno = Assert.Single(result.Value);
        Assert.Equal(1, turno.IdTurno);
        Assert.Equal("11223344", turno.DniPaciente);
        Assert.Equal("OSDE", turno.ObraSocialPaciente);
        Assert.Equal("Consultorio N° 101 (Piso 3, Sector Ala Norte)", turno.UbicacionConsultorio);
    }

    [Fact]
    public async Task Handle_MedicoInexistente_RetornaFallo()
    {
        var handler = new ObtenerTurnosPorMedicoQueryHandler(
            new FakeTurnoRepository(),
            new FakeRepository<Medico>());

        var result = await handler.Handle(new ObtenerTurnosPorMedicoQuery(999));

        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
    }

    private static Medico CrearMedico(int id)
    {
        return new Medico("Carlos", "Pérez", "22334455", new DateTime(1980, 1, 1), "carlos@test.com", Especialidad.Clinica, "MP-100")
        {
            IdPersona = id
        };
    }

    private static Paciente CrearPaciente(int id)
    {
        return new Paciente("Ana", "Ríos", "11223344", new DateTime(1990, 1, 1), "ana@test.com", "OSDE")
        {
            IdPersona = id
        };
    }
}
