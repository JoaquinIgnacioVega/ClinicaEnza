using System;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests
{
    public class PersonaTests
    {
        private class PersonaPrueba : Persona
        {
            public PersonaPrueba(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail)
                : base(nombre, apellido, dni, fechaNacimiento, mail) { }

            public override string ObtenerIdentificacion() => $"DNI: {Dni}";
        }

        [Fact]
        public void CalcularEdad_CasoNormal()
        {
            var fechaNacimiento = DateTime.Today.AddYears(-25);
            var persona = new PersonaPrueba("Joaquín", "Vega", "12345678", fechaNacimiento, "joaquin@test.com");

            var edad = persona.CalcularEdad();
            var nombreCompleto = persona.ObtenerNombreCompleto();

            Assert.Equal(25, edad);
            Assert.Equal("VEGA, Joaquín", nombreCompleto);
        }

        [Fact]
        public void CalcularEdad_CumpleAniosHoy()
        {
            var hoy = DateTime.Today;
            var fechaNacimiento = hoy.AddYears(-20);
            var persona = new PersonaPrueba("María", "Gómez", "87654321", fechaNacimiento, "maria@test.com");

            var edad = persona.CalcularEdad();

            Assert.Equal(20, edad);
        }

        [Theory]
        [InlineData("", "Vega", "12345678")]
        [InlineData("Joaquín", "", "12345678")]
        [InlineData("Joaquín", "Vega", "")]
        public void Constructor_DatosVacios_LanzaExcepcion(string nombre, string apellido, string dni)
        {
            Assert.Throws<ArgumentException>(() =>
                new PersonaPrueba(nombre, apellido, dni, DateTime.Today.AddYears(-20), "mail@test.com")
            );
        }
    }
}