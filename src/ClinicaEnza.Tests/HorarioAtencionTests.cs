using System;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests
{
    public class HorarioAtencionTests
    {
        private Medico CrearMedicoMock() =>
            new Medico("Esteban", "Quito", "11223344", DateTime.Today.AddYears(-40), "esteban@test.com", Especialidad.Clinica, "MP-8888");

        private Consultorio CrearConsultorioMock() =>
            new Consultorio("101", 1, "Ala Norte");

        [Fact]
        public void IncluyeHorario_EnRango_RetornaTrue()
        {
            var horario = new HorarioAtencion(
                DayOfWeek.Monday,
                new TimeSpan(8, 0, 0),
                new TimeSpan(12, 0, 0),
                CrearMedicoMock(),
                CrearConsultorioMock()
            );

            var fechaLunes = new DateTime(2026, 10, 19, 9, 30, 0);

            var resultado = horario.IncluyeHorario(fechaLunes);

            Assert.True(resultado);
        }

        [Fact]
        public void IncluyeHorario_HoraLimite_RetornaFalse()
        {
            var horario = new HorarioAtencion(
                DayOfWeek.Monday,
                new TimeSpan(8, 0, 0),
                new TimeSpan(12, 0, 0),
                CrearMedicoMock(),
                CrearConsultorioMock()
            );

            var fechaLunesLimite = new DateTime(2026, 10, 19, 12, 0, 0);

            var resultado = horario.IncluyeHorario(fechaLunesLimite);

            Assert.False(resultado);
        }

        [Fact]
        public void Constructor_HoraHastaInvalida_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new HorarioAtencion(
                    DayOfWeek.Monday,
                    new TimeSpan(14, 0, 0),
                    new TimeSpan(10, 0, 0),
                    CrearMedicoMock(),
                    CrearConsultorioMock()
                )
            );
        }
    }
}