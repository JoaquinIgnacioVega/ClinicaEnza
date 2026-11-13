using System;
using ClinicaEnza.Domain;
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
}
