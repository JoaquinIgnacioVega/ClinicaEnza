using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Usuarios.Commands.DarDeBajaUsuario;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class DarDeBajaUsuarioCommandHandlerTests
{
    private readonly FakeUsuarioRepository _usuarioRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly DarDeBajaUsuarioHandler _handler;

    public DarDeBajaUsuarioCommandHandlerTests()
    {
        _usuarioRepository = new FakeUsuarioRepository();
        _unitOfWork = new FakeUnitOfWork(_usuarioRepository);
        _handler = new DarDeBajaUsuarioHandler(_usuarioRepository, _unitOfWork);
    }

    private Usuario CrearUsuarioEjemplo(int id = 1, bool activo = true)
    {
        var usuario = new Usuario(
            "Carlos",
            "Gómez",
            "30111222",
            new DateTime(1985, 2, 10),
            "carlos@clinicaenza.com",
            "USR-101",
            "hash_123",
            RolUsuario.Recepcionista)
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
    public async Task Handle_UsuarioExistenteYActivo_DaDeBajaYGuardaCambios()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo(1, activo: true);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new DarDeBajaUsuarioCommand(usuario.IdPersona);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(usuario.Activo);
        Assert.NotNull(usuario.FechaBaja);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _usuarioRepository.UpdateCallCount);
    }

    [Fact]
    public async Task Handle_UsuarioInexistente_RetornaFallo()
    {
        // Arrange
        var command = new DarDeBajaUsuarioCommand(999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el usuario con ID 999", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_UsuarioYaDadoDeBaja_RetornaFalloSinGuardar()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo(1, activo: false);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new DarDeBajaUsuarioCommand(usuario.IdPersona);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("El usuario ya se encuentra dado de baja", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_DarDeBajaUsuarioCommandHandlerAlias_FuncionaCorrectamente()
    {
        // Arrange
        var handlerAlias = new DarDeBajaUsuarioCommandHandler(_usuarioRepository, _unitOfWork);
        var usuario = CrearUsuarioEjemplo(2, activo: true);
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new DarDeBajaUsuarioCommand(usuario.IdPersona);

        // Act
        var result = await handlerAlias.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(usuario.Activo);
        Assert.NotNull(usuario.FechaBaja);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    #region Fakes

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();
        public int UpdateCallCount { get; private set; }

        public Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            Usuarios.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMailParaOtraPersonaAsync(string mail, int idPersonaExcluir, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase) && u.IdPersona != idPersonaExcluir));

        public Task<Usuario?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorMailAsync(string mail, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> ObtenerPorLegajoAsync(string legajo, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase)));

        public Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.IdPersona == id));

        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.ToList());

        public Task<IReadOnlyList<Usuario>> FindAsync(Expression<Func<Usuario, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.AsQueryable().Where(predicate).ToList());

        public void Update(Usuario entity)
        {
            UpdateCallCount++;
        }

        public void Delete(Usuario entity) => Usuarios.Remove(entity);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public IUsuarioRepository Usuarios { get; }
        public IMedicoRepository Medicos => throw new NotImplementedException();
        public int SaveChangesCallCount { get; private set; }

        public FakeUnitOfWork(IUsuarioRepository usuarioRepository)
        {
            Usuarios = usuarioRepository;
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    #endregion
}

