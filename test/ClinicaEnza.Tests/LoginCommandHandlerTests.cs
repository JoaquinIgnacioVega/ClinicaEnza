using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Features.Auth.Commands.Login;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class LoginCommandHandlerTests
{
    private readonly FakeUsuarioRepository _usuarioRepository;
    private readonly FakePasswordHasher _passwordHasher;
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator;
    private readonly LoginHandler _handler;

    public LoginCommandHandlerTests()
    {
        _usuarioRepository = new FakeUsuarioRepository();
        _passwordHasher = new FakePasswordHasher();
        _jwtTokenGenerator = new FakeJwtTokenGenerator();

        _handler = new LoginHandler(_usuarioRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_IdentificadorVacio_RetornaFallo()
    {
        // Arrange
        var command = new LoginCommand("", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("identificador", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_PasswordVacio_RetornaFallo()
    {
        // Arrange
        var command = new LoginCommand("admin@clinica.com", "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("contraseña", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaFallo()
    {
        // Arrange
        var command = new LoginCommand("inexistente@clinica.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Credenciales inválidas.", result.Error);
    }

    [Fact]
    public async Task Handle_UsuarioInactivo_RetornaFallo()
    {
        // Arrange
        var usuario = CrearUsuario("inactivo@clinica.com", "LEG-001", "hashed_Password123!", RolUsuario.Administrador);
        usuario.DarDeBaja();
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new LoginCommand("inactivo@clinica.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("activo", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_PasswordIncorrecto_RetornaFallo()
    {
        // Arrange
        var usuario = CrearUsuario("user@clinica.com", "LEG-002", "hashed_PasswordCorrecto!", RolUsuario.Recepcionista);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new LoginCommand("user@clinica.com", "PasswordErroneo");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Credenciales inválidas.", result.Error);
    }

    [Fact]
    public async Task Handle_CredencialesValidasPorMail_RetornaTokenYDatos()
    {
        // Arrange
        var usuario = CrearUsuario("admin@clinica.com", "LEG-003", "hashed_Secret123!", RolUsuario.Administrador);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new LoginCommand("admin@clinica.com", "Secret123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("fake_jwt_token_for_LEG-003", result.Value.Token);
        Assert.Equal(usuario.ObtenerNombreCompleto(), result.Value.NombreCompleto);
        Assert.Equal(RolUsuario.Administrador, result.Value.Rol);
        Assert.Null(result.Value.IdMedico);
    }

    [Fact]
    public async Task Handle_CredencialesValidasPorLegajo_RetornaTokenYDatos()
    {
        // Arrange
        var usuario = CrearUsuario("recep@clinica.com", "LEG-004", "hashed_Recep123!", RolUsuario.Recepcionista);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new LoginCommand("LEG-004", "Recep123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("fake_jwt_token_for_LEG-004", result.Value.Token);
        Assert.Equal(usuario.ObtenerNombreCompleto(), result.Value.NombreCompleto);
        Assert.Equal(RolUsuario.Recepcionista, result.Value.Rol);
        Assert.Null(result.Value.IdMedico);
    }

    [Fact]
    public async Task Handle_UsuarioConRolMedico_RetornaIdMedico()
    {
        // Arrange
        var usuario = CrearUsuario("medico@clinica.com", "LEG-005", "hashed_Med123!", RolUsuario.Medico, idMedico: 15);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new LoginCommand("LEG-005", "Med123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(RolUsuario.Medico, result.Value.Rol);
        Assert.Equal(15, result.Value.IdMedico);
    }

    private static Usuario CrearUsuario(
        string mail,
        string legajo,
        string passwordHash,
        RolUsuario rol,
        int? idMedico = null)
    {
        return new Usuario(
            nombre: "Juan",
            apellido: "Pérez",
            dni: "30123456",
            fechaNacimiento: new DateTime(1985, 5, 20),
            mail: mail,
            legajo: legajo,
            passwordHash: passwordHash,
            rol: rol,
            idMedico: idMedico);
    }

    #region Fakes

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();

        public Task<Usuario?> ObtenerPorMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailParaOtraPersonaAsync(string mail, int idPersonaExcluir, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase) && u.IdPersona != idPersonaExcluir));

        public Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.IdPersona == id));

        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.ToList());

        public Task<IReadOnlyList<Usuario>> FindAsync(Expression<Func<Usuario, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.AsQueryable().Where(predicate).ToList());

        public Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            Usuarios.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Usuario entity) { }
        public void Delete(Usuario entity) => Usuarios.Remove(entity);
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed_{password}";

        public bool VerifyPassword(string hashedPassword, string providedPassword) =>
            hashedPassword == $"hashed_{providedPassword}";
    }

    private class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(Usuario usuario) => $"fake_jwt_token_for_{usuario.Legajo}";
    }

    #endregion
}

