using System;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class UsuarioTests
{
    [Fact]
    public void CrearUsuario_ConDatosValidos_InstanciaCorrectamente()
    {
        // Arrange
        var fechaNacimiento = new DateTime(1990, 5, 20);
        var nombre = "Laura";
        var apellido = "Fernández";
        var dni = "35123456";
        var mail = "laura@clinicaenza.com";
        var legajo = "USR-001";
        var passwordHash = "hashed_password_123";
        var rol = RolUsuario.Administrador;

        // Act
        var usuario = new Usuario(
            nombre,
            apellido,
            dni,
            fechaNacimiento,
            mail,
            legajo,
            passwordHash,
            rol);

        // Assert
        Assert.Equal(nombre, usuario.Nombre);
        Assert.Equal(apellido, usuario.Apellido);
        Assert.Equal(dni, usuario.Dni);
        Assert.Equal(fechaNacimiento, usuario.FechaNacimiento);
        Assert.Equal(mail, usuario.Mail);
        Assert.Equal(legajo, usuario.Legajo);
        Assert.Equal(passwordHash, usuario.PasswordHash);
        Assert.Equal(rol, usuario.Rol);
        Assert.True(usuario.EsAdministrador());
        Assert.Null(usuario.IdMedico);
        Assert.Contains(legajo, usuario.ObtenerIdentificacion());
        Assert.Contains(rol.ToString(), usuario.ObtenerIdentificacion());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CrearUsuario_LegajoInvalido_LanzaArgumentException(string? legajoInvalido)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "Carlos",
                "Gómez",
                "28444555",
                new DateTime(1985, 3, 10),
                "carlos@clinicaenza.com",
                legajoInvalido!,
                "hash_123",
                RolUsuario.Recepcionista)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CrearUsuario_PasswordHashInvalido_LanzaArgumentException(string? hashInvalido)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "Carlos",
                "Gómez",
                "28444555",
                new DateTime(1985, 3, 10),
                "carlos@clinicaenza.com",
                "LEG-002",
                hashInvalido!,
                RolUsuario.Recepcionista)
        );
    }

    [Fact]
    public void CrearUsuario_RolMedicoSinMedico_LanzaArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "Martín",
                "López",
                "30111222",
                new DateTime(1982, 8, 15),
                "martin@clinicaenza.com",
                "MED-001",
                "hash_medico",
                RolUsuario.Medico,
                idMedico: null,
                medico: null)
        );
    }

    [Fact]
    public void CrearUsuario_RolMedicoConIdMedico_InstanciaCorrectamente()
    {
        // Arrange
        var idMedico = 10;

        // Act
        var usuario = new Usuario(
            "Martín",
            "López",
            "30111222",
            new DateTime(1982, 8, 15),
            "martin@clinicaenza.com",
            "MED-001",
            "hash_medico",
            RolUsuario.Medico,
            idMedico: idMedico);

        // Assert
        Assert.Equal(RolUsuario.Medico, usuario.Rol);
        Assert.False(usuario.EsAdministrador());
        Assert.Equal(idMedico, usuario.IdMedico);
    }

    [Fact]
    public void ActualizarPasswordHash_ConNuevoHash_ActualizaPropiedad()
    {
        // Arrange
        var usuario = new Usuario(
            "Laura",
            "Fernández",
            "35123456",
            new DateTime(1990, 5, 20),
            "laura@clinicaenza.com",
            "USR-001",
            "hash_inicial",
            RolUsuario.Administrador);

        // Act
        usuario.ActualizarPasswordHash("nuevo_hash_seguro");

        // Assert
        Assert.Equal("nuevo_hash_seguro", usuario.PasswordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ActualizarPasswordHash_Vacio_LanzaArgumentException(string? nuevoHashInvalido)
    {
        // Arrange
        var usuario = new Usuario(
            "Laura",
            "Fernández",
            "35123456",
            new DateTime(1990, 5, 20),
            "laura@clinicaenza.com",
            "USR-001",
            "hash_inicial",
            RolUsuario.Administrador);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => usuario.ActualizarPasswordHash(nuevoHashInvalido!));
    }
}

