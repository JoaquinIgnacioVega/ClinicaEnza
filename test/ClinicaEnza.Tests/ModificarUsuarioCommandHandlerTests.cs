using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Usuarios.Commands.ModificarUsuario;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class ModificarUsuarioCommandHandlerTests
{
    private readonly FakeUsuarioRepository _usuarioRepository;
    private readonly FakeMedicoRepository _medicoRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly ModificarUsuarioCommandHandler _handler;

    public ModificarUsuarioCommandHandlerTests()
    {
        _usuarioRepository = new FakeUsuarioRepository();
        _medicoRepository = new FakeMedicoRepository();
        _unitOfWork = new FakeUnitOfWork(_usuarioRepository, _medicoRepository);

        _handler = new ModificarUsuarioCommandHandler(
            _usuarioRepository,
            _unitOfWork,
            _medicoRepository);
    }

    private Usuario CrearUsuarioEjemplo(int id = 1, string mail = "usuario@clinicaenza.com")
    {
        return new Usuario(
            "Pablo",
            "Marmol",
            "30111222",
            new DateTime(1985, 2, 10),
            mail,
            "USR-101",
            "hash_123",
            RolUsuario.Recepcionista)
        {
            IdPersona = id
        };
    }

    [Fact]
    public async Task Handle_UsuarioExistenteYDatosValidos_ModificaYGuardaCambios()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new ModificarUsuarioCommand(
            idUsuario: usuario.IdPersona,
            nombre: "Pablo Andrés",
            apellido: "Mármol Picapiedra",
            mail: "pablo.marmol@clinicaenza.com",
            rol: RolUsuario.Administrador,
            idMedico: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pablo Andrés", result.Value.Nombre);
        Assert.Equal("Mármol Picapiedra", result.Value.Apellido);
        Assert.Equal("pablo.marmol@clinicaenza.com", result.Value.Mail);
        Assert.Equal(RolUsuario.Administrador, result.Value.Rol);
        Assert.Null(result.Value.IdMedico);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaFallo()
    {
        // Arrange
        var command = new ModificarUsuarioCommand(
            idUsuario: 999,
            nombre: "Pablo",
            apellido: "Marmol",
            mail: "pablo@clinicaenza.com",
            rol: RolUsuario.Recepcionista);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el usuario con ID 999", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MailPertenecienteAOtraPersona_RetornaFalloSinGuardar()
    {
        // Arrange
        var usuario1 = CrearUsuarioEjemplo(1, "usuario1@clinicaenza.com");
        var usuario2 = CrearUsuarioEjemplo(2, "usuario2@clinicaenza.com");
        _usuarioRepository.Usuarios.Add(usuario1);
        _usuarioRepository.Usuarios.Add(usuario2);

        var command = new ModificarUsuarioCommand(
            idUsuario: 1,
            nombre: "Pablo",
            apellido: "Marmol",
            mail: "usuario2@clinicaenza.com",
            rol: RolUsuario.Recepcionista);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("ya se encuentra registrado por otra persona", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MismoMailDelUsuario_PermiteModificacion()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo(1, "mismo.mail@clinicaenza.com");
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new ModificarUsuarioCommand(
            idUsuario: 1,
            nombre: "Pablo Nuevo",
            apellido: "Marmol",
            mail: "mismo.mail@clinicaenza.com",
            rol: RolUsuario.Recepcionista);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pablo Nuevo", result.Value.Nombre);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoSinIdMedico_RetornaFallo()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new ModificarUsuarioCommand(
            idUsuario: usuario.IdPersona,
            nombre: "Pablo",
            apellido: "Marmol",
            mail: "pablo@clinicaenza.com",
            rol: RolUsuario.Medico,
            idMedico: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Médico", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoConMedicoInexistente_RetornaFallo()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        _usuarioRepository.Usuarios.Add(usuario);

        var command = new ModificarUsuarioCommand(
            idUsuario: usuario.IdPersona,
            nombre: "Pablo",
            apellido: "Marmol",
            mail: "pablo@clinicaenza.com",
            rol: RolUsuario.Medico,
            idMedico: 888);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoConMedicoExistente_ModificaCorrectamente()
    {
        // Arrange
        var usuario = CrearUsuarioEjemplo();
        _usuarioRepository.Usuarios.Add(usuario);

        var medico = new Medico(
            "Valeria",
            "Rojas",
            "25888999",
            new DateTime(1978, 6, 20),
            "valeria@clinicaenza.com",
            Especialidad.Pediatria,
            "MP-9911")
        {
            IdPersona = 10
        };
        _medicoRepository.Medicos.Add(medico);

        var command = new ModificarUsuarioCommand(
            idUsuario: usuario.IdPersona,
            nombre: "Pablo",
            apellido: "Marmol",
            mail: "pablo@clinicaenza.com",
            rol: RolUsuario.Medico,
            idMedico: medico.IdPersona);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(RolUsuario.Medico, result.Value.Rol);
        Assert.Equal(medico.IdPersona, result.Value.IdMedico);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("", "Marmol", "pablo@clinicaenza.com")]
    [InlineData("Pablo", "", "pablo@clinicaenza.com")]
    [InlineData("Pablo", "Marmol", "")]
    public async Task Handle_CamposObligatoriosVacios_RetornaFallo(string nombre, string apellido, string mail)
    {
        // Arrange
        var command = new ModificarUsuarioCommand(
            idUsuario: 1,
            nombre: nombre,
            apellido: apellido,
            mail: mail,
            rol: RolUsuario.Recepcionista);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    #region Fakes

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();

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

        public void Update(Usuario entity) { }
        public void Delete(Usuario entity) => Usuarios.Remove(entity);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public IUsuarioRepository Usuarios { get; }
        public IMedicoRepository Medicos { get; }
        public int SaveChangesCallCount { get; private set; }

        public FakeUnitOfWork(IUsuarioRepository usuarioRepository, IMedicoRepository medicoRepository)
        {
            Usuarios = usuarioRepository;
            Medicos = medicoRepository;
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

    private class FakeMedicoRepository : IMedicoRepository
    {
        public List<Medico> Medicos { get; } = new();

        public Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.FirstOrDefault(m => m.IdPersona == id));

        public Task<IReadOnlyList<Medico>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.ToList());

        public Task<IReadOnlyList<Medico>> FindAsync(Expression<Func<Medico, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.AsQueryable().Where(predicate).ToList());

        public Task AddAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            Medicos.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Medico entity) { }
        public void Delete(Medico entity) => Medicos.Remove(entity);

        public Task<bool> ExisteMatriculaAsync(string matricula, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.Any(m => string.Equals(m.Matricula, matricula, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExisteMatriculaAsync(string matricula, int? idMedicoExcluir, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.Any(m => string.Equals(m.Matricula, matricula, StringComparison.OrdinalIgnoreCase) && (!idMedicoExcluir.HasValue || m.IdPersona != idMedicoExcluir.Value)));
        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.Any(m => string.Equals(m.Dni, dni, StringComparison.OrdinalIgnoreCase)));
    }

    #endregion
}

