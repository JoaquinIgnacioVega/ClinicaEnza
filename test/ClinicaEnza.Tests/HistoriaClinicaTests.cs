using System;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests
{
    public class HistoriaClinicaTests
    {
        private Paciente CrearPacienteMock() =>
            new Paciente("Lucía", "Méndez", "33445566", DateTime.Today.AddYears(-28), "lucia@test.com", "OSDE");

        private Medico CrearMedicoMock() =>
            new Medico("Roberto", "García", "44556677", DateTime.Today.AddYears(-45), "roberto@test.com", Especialidad.Cardiologia, "MP-9999");

        [Fact]
        public void AgregarPrescripcion_Valida_AgregaALaLista()
        {
            var historia = new HistoriaClinica(CrearPacienteMock());
            var prescripcion = new PrescripcionMedica("Aspirina 100mg", historia, CrearMedicoMock());

            historia.AgregarPrescripcion(prescripcion);

            Assert.Single(historia.Prescripciones);
        }

        [Fact]
        public void AgregarPrescripcion_ConIdMedicoYMedicamento_CreaYAgregaALaLista()
        {
            var historia = new HistoriaClinica(CrearPacienteMock()) { IdHistoriaClinica = 5 };
            var medico = CrearMedicoMock();
            medico.IdPersona = 2;

            var prescripcion = historia.AgregarPrescripcion(2, "Amoxicilina 500mg", medico);

            Assert.NotNull(prescripcion);
            Assert.Single(historia.Prescripciones);
            Assert.Equal("Amoxicilina 500mg", prescripcion.Medicamento);
            Assert.Equal(2, prescripcion.IdMedico);
            Assert.Equal(5, prescripcion.IdHistoriaClinica);
            Assert.Equal(DateTime.Today, prescripcion.Fecha.Date);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AgregarPrescripcion_MedicamentoVacio_LanzaArgumentException(string medicamentoInvalido)
        {
            var historia = new HistoriaClinica(CrearPacienteMock());

            Assert.Throws<ArgumentException>(() => historia.AgregarPrescripcion(1, medicamentoInvalido));
        }

        [Fact]
        public void AgregarPrescripcion_IdMedicoInvalido_LanzaArgumentException()
        {
            var historia = new HistoriaClinica(CrearPacienteMock());

            Assert.Throws<ArgumentException>(() => historia.AgregarPrescripcion(0, "Paracetamol 500mg"));
        }

        [Fact]
        public void BuscarPrimeraPrescripcion_Coincidencia_RetornaPrescripcion()
        {
            var historia = new HistoriaClinica(CrearPacienteMock());
            var medico = CrearMedicoMock();
            historia.AgregarPrescripcion(new PrescripcionMedica("Ibuprofeno 400mg", historia, medico));

            var encontrada = historia.BuscarPrimeraPrescripcionDe("Ibuprofeno 400mg");

            Assert.NotNull(encontrada);
            Assert.Equal("Ibuprofeno 400mg", encontrada.Medicamento);
        }

        [Fact]
        public void AgregarPrescripcion_Nula_LanzaExcepcion()
        {
            var historia = new HistoriaClinica(CrearPacienteMock());

            Assert.Throws<ArgumentNullException>(() => historia.AgregarPrescripcion(null!));
        }
    }
}
