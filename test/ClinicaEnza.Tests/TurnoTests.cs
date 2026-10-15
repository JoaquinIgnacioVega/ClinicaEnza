using System;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests
{
    public class TurnoTests
    {
        private Paciente CrearPacienteMock() =>
            new Paciente("Ana", "Ríos", "11223344", DateTime.Today.AddYears(-28), "ana@test.com", "OSDE");

        private Medico CrearMedicoMock() =>
            new Medico("Carlos", "Pérez", "22334455", DateTime.Today.AddYears(-40), "carlos@test.com", Especialidad.Clinica, "MP-100");

        private Consultorio CrearConsultorioMock() =>
            new Consultorio("102", 1, "Ala Norte");

        [Fact]
        public void CrearTurno_CalculaDuracion20Minutos()
        {
            var horaInicio = DateTime.Now.AddDays(1);

            var turno = new Turno(horaInicio, CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            Assert.Equal(horaInicio, turno.HoraInicio);
            Assert.Equal(horaInicio.AddMinutes(20), turno.HoraFin);
            Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        }

        [Fact]
        public void EsSolapadoCon_TurnosConsecutivos_RetornaFalse()
        {
            var horaBase = DateTime.Now.AddDays(1);
            var turno1 = new Turno(horaBase, CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            var turno2 = new Turno(horaBase.AddMinutes(20), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            var estaSolapado = turno1.EsSolapadoCon(turno2);

            Assert.False(estaSolapado);
        }

        [Fact]
        public void Reagendar_FechaPasada_LanzaExcepcion()
        {
            var turno = new Turno(DateTime.Now.AddDays(2), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            Assert.Throws<InvalidOperationException>(() =>
                turno.Reagendar(DateTime.Now.AddDays(-1))
            );
        }
    }
}
