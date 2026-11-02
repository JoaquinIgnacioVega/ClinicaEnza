using System;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Exceptions;
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
        public void EsSolapadoCon_TurnoCancelado_RetornaFalse()
        {
            var horaBase = DateTime.Now.AddDays(1);
            var turnoActivo = new Turno(horaBase, CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            var turnoCancelado = new Turno(horaBase.AddMinutes(5), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            turnoCancelado.CambiarEstado(EstadoTurno.Cancelado);

            Assert.False(turnoActivo.EsSolapadoCon(turnoCancelado));
            Assert.False(turnoCancelado.EsSolapadoCon(turnoActivo));
        }

        [Fact]
        public void Reagendar_FechaPasada_LanzaExcepcion()
        {
            var turno = new Turno(DateTime.Now.AddDays(2), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            Assert.Throws<ReglaDeNegocioException>(() =>
                turno.Reagendar(DateTime.Now.AddDays(-1))
            );
        }

        [Fact]
        public void Reagendar_TurnoPendiente_ActualizaHorarioYDuracion()
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            var nuevaHora = DateTime.Now.AddDays(2);

            turno.Reagendar(nuevaHora);

            Assert.Equal(nuevaHora, turno.HoraInicio);
            Assert.Equal(nuevaHora.AddMinutes(20), turno.HoraFin);
            Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        }

        [Fact]
        public void Reagendar_TurnoCancelado_LanzaExcepcion()
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            turno.CambiarEstado(EstadoTurno.Cancelado);

            Assert.Throws<ReglaDeNegocioException>(() =>
                turno.Reagendar(DateTime.Now.AddDays(2)));
        }

        [Fact]
        public void Reagendar_ConNuevoConsultorio_ActualizaConsultorio()
        {
            var consultorioAnterior = CrearConsultorioMock();
            var nuevoConsultorio = new Consultorio("202", 2, "Ala Sur");
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), consultorioAnterior);

            turno.Reagendar(DateTime.Now.AddDays(2), nuevoConsultorio);

            Assert.Equal(nuevoConsultorio.IdConsultorio, turno.IdConsultorio);
            Assert.Same(nuevoConsultorio, turno.Consultorio);
        }

        [Fact]
        public void Cancelar_TurnoPendiente_CambiaEstado()
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            turno.Cancelar();

            Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
        }

        [Theory]
        [InlineData(EstadoTurno.Cancelado)]
        [InlineData(EstadoTurno.Realizado)]
        public void Cancelar_TurnoNoPendiente_LanzaExcepcion(EstadoTurno estado)
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            turno.CambiarEstado(estado);

            Assert.Throws<ReglaDeNegocioException>(() => turno.Cancelar());
        }

        [Fact]
        public void MarcarComoRealizado_TurnoPendiente_CambiaEstado()
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());

            turno.MarcarComoRealizado();

            Assert.Equal(EstadoTurno.Realizado, turno.Estado);
        }

        [Theory]
        [InlineData(EstadoTurno.Cancelado)]
        [InlineData(EstadoTurno.Realizado)]
        public void MarcarComoRealizado_TurnoNoPendiente_LanzaExcepcion(EstadoTurno estado)
        {
            var turno = new Turno(DateTime.Now.AddDays(1), CrearPacienteMock(), CrearMedicoMock(), CrearConsultorioMock());
            turno.CambiarEstado(estado);

            Assert.Throws<ReglaDeNegocioException>(() => turno.MarcarComoRealizado());
        }
    }
}
