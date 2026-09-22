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

        // Caso normal: Creación de turno calcula automáticamente la duración de 20 minutos
        [Fact]
        public void Turno_CasoNormal_DebeCalcularHoraFinA20Minutos()
        {
            var horaInicio = DateTime.Now.AddDays(1);

            var turno = new Turno(horaInicio, CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            Assert.Equal(horaInicio, turno.HoraInicio);
            Assert.Equal(horaInicio.AddMinutes(20), turno.HoraFin);
            Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        }

        // Caso borde: Verificar turnos contiguos (No deben considerarse solapados)
        [Fact]
        public void Turno_CasoBorde_TurnosConsecutivos_NoDebeExistirSolapamiento()
        {
            var horaBase = DateTime.Now.AddDays(1);
            var turno1 = new Turno(horaBase, CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            var turno2 = new Turno(horaBase.AddMinutes(20), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            var estaSolapado = turno1.EsSolapadoCon(turno2);

            Assert.False(estaSolapado);
        }

        // Caso de error: Intentar reagendar un turno a una fecha/hora del pasado
        [Fact]
        public void Turno_CasoError_ReagendarAFechaPasada_DebeLanzarInvalidOperationException()
        {
            var turno = new Turno(DateTime.Now.AddDays(2), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            Assert.Throws<InvalidOperationException>(() =>
                turno.Reagendar(DateTime.Now.AddDays(-1))
            );
        }
    }
}
