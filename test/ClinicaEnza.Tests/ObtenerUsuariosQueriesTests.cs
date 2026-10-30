using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;
using ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarios;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerUsuariosQueriesTests
{
    private readonly FakeUsuarioRepository _usuarioRepository;
    private readonly ObtenerUsuariosHandler _obtenerUsuariosHandler;
    private readonly ObtenerUsuarioPorIdHandler _obtenerUsuarioPorIdHandler;

    public ObtenerUsuariosQueriesTests()
    {
        _usuarioRepository = new FakeUsuarioRepository();
        _obtenerUsuariosHandler = new ObtenerUsuariosHandler(_usuarioRepository);
        _obtenerUsuarioPorIdHandler = new ObtenerUsuarioPorIdHandler(_usuarioRepository);
    }

    private void CargarUsuariosDePrueba()
    {
        _usuarioRepository.Usuarios.AddRange(new[]
        {
            CrearUsuario(1, "Ana", "Perez", "30111222", "ana@test.com", "USR-101", RolUsuario.Recepcionista, activo: true),
            CrearUsuario(2, "Carlos", "Gomez", "32333444", "carlos@test.com", "MED-201", RolUsuario.Medico, activo: true, idMedico: 10),
            CrearUsuario(3, "Laura", "Diaz", "34555666", "laura@test.com", "ADM-301", RolUsuario.Administrador, activo: false),
            CrearUsuario(4, "Marcos", "Ruiz", "36777888", "marcos@test.com", "MED-202", RolUsuario.Medico, activo: false, idMedico: 20)
        });
    }

    private static Usuario CrearUsuario(
        int id,
        string nombre,
        string apellido,
        string dni,
        string mail,
        string legajo,
        RolUsuario rol,
        bool activo = true,
        int? idMedico = null)
    {
        var usuario = new Usuario(
            nombre,
            apellido,
            dni,
            new DateTime(1990, 5, 15),
            mail,
            legajo,
            "hash_123",
            rol,
            idMedico)
        {
            IdPersona = id
        };

        if (!activo)
        {
            usuario.DarDeBaja();
        }

        return usuario;
    }

    [Fact]
    public async Task ObtenerUsuarios_SinFiltros_RetornaTodosLosUsuarios()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery();

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Count);
    }

    [Fact]
    public async Task ObtenerUsuarios_FiltroSoloActivosTrue_RetornaSoloActivos()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(SoloActivos: true);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, u => Assert.True(u.Activo));
    }

    [Fact]
    public async Task ObtenerUsuarios_FiltroSoloActivosFalse_RetornaSoloInactivos()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(SoloActivos: false);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, u => Assert.False(u.Activo));
    }

    [Fact]
    public async Task ObtenerUsuarios_FiltroRol_RetornaSoloUsuariosConEseRol()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(Rol: RolUsuario.Medico);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, u => Assert.Equal(RolUsuario.Medico, u.Rol));
    }

    [Fact]
    public async Task ObtenerUsuarios_FiltroSoloActivosYRol_AplicaAmbosFiltros()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(SoloActivos: true, Rol: RolUsuario.Medico);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var usuario = Assert.Single(result.Value);
        Assert.Equal("Carlos", usuario.Nombre);
        Assert.True(usuario.Activo);
        Assert.Equal(RolUsuario.Medico, usuario.Rol);
    }

    [Fact]
    public async Task ObtenerUsuarios_FiltroSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(SoloActivos: false, Rol: RolUsuario.Recepcionista);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task ObtenerUsuarios_ProyectaCamposCorrectamente()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuariosQuery(SoloActivos: true, Rol: RolUsuario.Medico);

        // Act
        var result = await _obtenerUsuariosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var usuario = Assert.Single(result.Value);
        Assert.Equal(2, usuario.IdPersona);
        Assert.Equal(2, usuario.IdUsuario);
        Assert.Equal("GOMEZ, Carlos", usuario.NombreCompleto);
        Assert.Equal("32333444", usuario.Dni);
        Assert.Equal("carlos@test.com", usuario.Mail);
        Assert.Equal("MED-201", usuario.Legajo);
        Assert.Equal(RolUsuario.Medico, usuario.Rol);
        Assert.True(usuario.Activo);
        Assert.Equal(10, usuario.IdMedico);
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_Existente_RetornaUsuarioDto()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuarioPorIdQuery(1);

        // Act
        var result = await _obtenerUsuarioPorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.IdPersona);
        Assert.Equal("PEREZ, Ana", result.Value.NombreCompleto);
        Assert.Equal("30111222", result.Value.Dni);
        Assert.Equal("ana@test.com", result.Value.Mail);
        Assert.Equal("USR-101", result.Value.Legajo);
        Assert.Equal(RolUsuario.Recepcionista, result.Value.Rol);
        Assert.True(result.Value.Activo);
        Assert.Null(result.Value.IdMedico);
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_Inexistente_RetornaFalloNotFound()
    {
        // Arrange
        CargarUsuariosDePrueba();
        var query = new ObtenerUsuarioPorIdQuery(999);

        // Act
        var result = await _obtenerUsuarioPorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el usuario con ID 999", result.Error);
    }

    #region Fakes

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();

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

        public Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailParaOtraPersonaAsync(string mail, int idPersonaExcluir, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase) && u.IdPersona != idPersonaExcluir));
    }

    #endregion
}
