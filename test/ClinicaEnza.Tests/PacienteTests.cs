using System;
using ClinicaEnza.Domain;
using Xunit;

namespace ClinicaEnza.Tests;

public class PacienteTests
{
    [Fact]
    public void InstanciarPaciente_ConDatosCompletos_InicializaHistoriaClinicaYPropiedades()
    {
        // Arrange
        var nombre = "Juan";
        var apellido = "Pérez";
        var dni = "12345678";
        var fechaNacimiento = DateTime.Today.AddYears(-30);
        var mail = "juan.perez@test.com";
        var obraSocial = "OSDE";
        var peso = 75.5m;
        var altura = 1.78m;

        // Act
        var paciente = new Paciente(nombre, apellido, dni, fechaNacimiento, mail, obraSocial, peso, altura);

        // Assert
        Assert.Equal(nombre, paciente.Nombre);
        Assert.Equal(apellido, paciente.Apellido);
        Assert.Equal(dni, paciente.Dni);
        Assert.Equal(fechaNacimiento, paciente.FechaNacimiento);
        Assert.Equal(mail, paciente.Mail);
        Assert.Equal(obraSocial, paciente.ObraSocial);
        Assert.Equal(peso, paciente.Peso);
        Assert.Equal(altura, paciente.Altura);
        Assert.NotNull(paciente.HistoriaClinica);
        Assert.Same(paciente, paciente.HistoriaClinica.Paciente);
    }

    [Fact]
    public void InstanciarPaciente_ConstructorBasico_InicializaHistoriaClinica()
    {
        // Arrange & Act
        var paciente = new Paciente(
            "Ana",
            "Gómez",
            "23456789",
            DateTime.Today.AddYears(-25),
            "ana.gomez@test.com",
            "Swiss Medical");

        // Assert
        Assert.NotNull(paciente.HistoriaClinica);
        Assert.Same(paciente, paciente.HistoriaClinica.Paciente);
        Assert.Equal(0m, paciente.Peso);
        Assert.Equal(0m, paciente.Altura);
    }

    [Fact]
    public void InicializarHistoriaClinica_CuandoYaExiste_NoReemplazaInstancia()
    {
        // Arrange
        var paciente = new Paciente(
            "Juan",
            "Pérez",
            "12345678",
            DateTime.Today.AddYears(-30),
            "juan@test.com",
            "OSDE",
            70m,
            1.75m);

        var historiaInicial = paciente.HistoriaClinica;

        // Act
        paciente.InicializarHistoriaClinica();

        // Assert
        Assert.Same(historiaInicial, paciente.HistoriaClinica);
    }

    [Theory]
    [InlineData("", "Pérez", "12345678", "juan@test.com", "OSDE")]
    [InlineData("   ", "Pérez", "12345678", "juan@test.com", "OSDE")]
    [InlineData("Juan", "", "12345678", "juan@test.com", "OSDE")]
    [InlineData("Juan", "   ", "12345678", "juan@test.com", "OSDE")]
    [InlineData("Juan", "Pérez", "", "juan@test.com", "OSDE")]
    [InlineData("Juan", "Pérez", "   ", "juan@test.com", "OSDE")]
    [InlineData("Juan", "Pérez", "12345678", "", "OSDE")]
    [InlineData("Juan", "Pérez", "12345678", "   ", "OSDE")]
    [InlineData("Juan", "Pérez", "12345678", "juan@test.com", "")]
    [InlineData("Juan", "Pérez", "12345678", "juan@test.com", "   ")]
    public void InstanciarPaciente_DatosObligatoriosVacios_LanzaArgumentException(
        string nombre, string apellido, string dni, string mail, string obraSocial)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Paciente(nombre, apellido, dni, DateTime.Today.AddYears(-20), mail, obraSocial, 70m, 1.75m));
    }

    [Fact]
    public void InstanciarPaciente_FechaNacimientoDefault_LanzaArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Paciente("Juan", "Pérez", "12345678", default, "juan@test.com", "OSDE", 70m, 1.75m));
    }

    [Theory]
    [InlineData(0, 1.75)]
    [InlineData(-5, 1.75)]
    [InlineData(70, 0)]
    [InlineData(70, -1.75)]
    public void InstanciarPaciente_PesoOAlturaNoPositivos_LanzaArgumentException(decimal peso, decimal altura)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Paciente(
                "Juan",
                "Pérez",
                "12345678",
                DateTime.Today.AddYears(-25),
                "juan@test.com",
                "OSDE",
                peso,
                altura));
    }

    [Fact]
    public void ActualizarMedidas_ValoresPositivos_ActualizaPropiedades()
    {
        // Arrange
        var paciente = new Paciente(
            "Juan",
            "Pérez",
            "12345678",
            DateTime.Today.AddYears(-25),
            "juan@test.com",
            "OSDE",
            70m,
            1.75m);

        // Act
        paciente.ActualizarMedidas(78.5m, 1.76m);

        // Assert
        Assert.Equal(78.5m, paciente.Peso);
        Assert.Equal(1.76m, paciente.Altura);
    }

    [Theory]
    [InlineData(0, 1.75)]
    [InlineData(-10, 1.75)]
    [InlineData(70, 0)]
    [InlineData(70, -1.80)]
    public void ActualizarMedidas_ValoresNoPositivos_LanzaArgumentException(decimal peso, decimal altura)
    {
        // Arrange
        var paciente = new Paciente(
            "Juan",
            "Pérez",
            "12345678",
            DateTime.Today.AddYears(-25),
            "juan@test.com",
            "OSDE",
            70m,
            1.75m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => paciente.ActualizarMedidas(peso, altura));
    }

    [Fact]
    public void ObtenerIdentificacion_RetornaFormatoCorrecto()
    {
        // Arrange
        var paciente = new Paciente(
            "Juan",
            "Pérez",
            "12345678",
            DateTime.Today.AddYears(-25),
            "juan@test.com",
            "OSDE",
            70m,
            1.75m);

        // Act
        var identificacion = paciente.ObtenerIdentificacion();

        // Assert
        Assert.Equal("Paciente DNI: 12345678 | Obra Social: OSDE", identificacion);
    }
}

