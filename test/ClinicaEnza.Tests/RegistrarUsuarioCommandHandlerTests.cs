using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Features.Usuarios.Commands.RegistrarUsuario;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class RegistrarUsuarioCommandHandlerTests
{
    private readonly FakeUsuarioRepository _usuarioRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakePasswordHasher _passwordHasher;
    private readonly FakeMedicoRepository _medicoRepository;
    private readonly RegistrarUsuarioCommandHandler _handler;

    public RegistrarUsuarioCommandHandlerTests()
    {
        _usuarioRepository = new FakeUsuarioRepository();
        _unitOfWork = new FakeUnitOfWork(_usuarioRepository);
        _passwordHasher = new FakePasswordHasher();
        _medicoRepository = new FakeMedicoRepository();

        _handler = new RegistrarUsuarioCommandHandler(
            _usuarioRepository,
            _unitOfWork,
            _passwordHasher,
            _medicoRepository);
    }

    private RegistrarUsuarioCommand CrearComandoValido()
    {
        return new RegistrarUsuarioCommand(
            nombre: "Marcos",
            apellido: "Paz",
            dni: "34999888",
            fechaNacimiento: new DateTime(1989, 4, 12),
            mail: "marcos@clinicaenza.com",
            legajo: "LEG-100",
            password: "SecretPassword123!",
            rol: RolUsuario.Administrador);
    }

    [Fact]
    public async Task Handle_DatosValidos_RegistraUsuarioYConfirmaTransaccion()
    {
        // Arrange
        var command = CrearComandoValido();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(command.Legajo, result.Value.Legajo);
        Assert.Equal(command.Mail, result.Value.Mail);
        Assert.Equal(command.Rol, result.Value.Rol);
        Assert.Single(_usuarioRepository.Usuarios);
        Assert.Equal("hashed_SecretPassword123!", _usuarioRepository.Usuarios[0].PasswordHash);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_DniDuplicado_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        _usuarioRepository.DnisExistentes.Add(command.Dni);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("DNI", result.Error);
        Assert.Empty(_usuarioRepository.Usuarios);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_MailDuplicado_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        _usuarioRepository.MailsExistentes.Add(command.Mail);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("correo electrónico", result.Error);
        Assert.Empty(_usuarioRepository.Usuarios);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_LegajoDuplicado_RetornaFalloSinGuardar()
    {
        // Arrange
        var command = CrearComandoValido();
        _usuarioRepository.LegajosExistentes.Add(command.Legajo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("legajo", result.Error);
        Assert.Empty(_usuarioRepository.Usuarios);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoSinIdMedico_RetornaFallo()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rol = RolUsuario.Medico;
        command.IdMedico = null;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Médico", result.Error);
        Assert.Empty(_usuarioRepository.Usuarios);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoConIdMedicoInexistente_RetornaFallo()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rol = RolUsuario.Medico;
        command.IdMedico = 999;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico", result.Error);
        Assert.Empty(_usuarioRepository.Usuarios);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_RolMedicoConIdMedicoExistente_RegistraUsuarioConMedicoAsociado()
    {
        // Arrange
        var medico = new Medico(
            "Andrea",
            "Suárez",
            "20111222",
            new DateTime(1975, 11, 3),
            "andrea@clinicaenza.com",
            Especialidad.Cardiologia,
            "MP-8899",
            new DateTime(2010, 3, 1))
        {
            IdPersona = 5
        };
        _medicoRepository.Medicos.Add(medico);

        var command = CrearComandoValido();
        command.Rol = RolUsuario.Medico;
        command.IdMedico = medico.IdPersona;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(medico.IdPersona, result.Value.IdMedico);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Theory]
    [InlineData("", "Paz", "34999888", "test@test.com", "LEG-1", "pass")]
    [InlineData("Marcos", "", "34999888", "test@test.com", "LEG-1", "pass")]
    [InlineData("Marcos", "Paz", "", "test@test.com", "LEG-1", "pass")]
    [InlineData("Marcos", "Paz", "34999888", "", "LEG-1", "pass")]
    [InlineData("Marcos", "Paz", "34999888", "test@test.com", "", "pass")]
    [InlineData("Marcos", "Paz", "34999888", "test@test.com", "LEG-1", "")]
    public async Task Handle_CamposObligatoriosVacios_RetornaFallo(
        string nombre, string apellido, string dni, string mail, string legajo, string password)
    {
        // Arrange
        var command = new RegistrarUsuarioCommand(
            nombre, apellido, dni, DateTime.Today.AddYears(-20), mail, legajo, password, RolUsuario.Recepcionista);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ErrorAlGuardar_EjecutaRollbackTransaction()
    {
        // Arrange
        var command = CrearComandoValido();
        _unitOfWork.ThrowOnSave = true;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.RollbackCallCount);
    }

    #region Fakes

    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = new();
        public HashSet<string> DnisExistentes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> MailsExistentes { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> LegajosExistentes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            Usuarios.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default)
        {
            var existe = DnisExistentes.Contains(dni) || Usuarios.Any(u => string.Equals(u.Dni, dni, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

        public Task<bool> ExisteMailAsync(string mail, CancellationToken cancellationToken = default)
        {
            var existe = MailsExistentes.Contains(mail) || Usuarios.Any(u => string.Equals(u.Mail, mail, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

        public Task<bool> ExisteLegajoAsync(string legajo, CancellationToken cancellationToken = default)
        {
            var existe = LegajosExistentes.Contains(legajo) || Usuarios.Any(u => string.Equals(u.Legajo, legajo, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

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
        public IMedicoRepository Medicos { get; } = new FakeMedicoRepository();
        public int SaveChangesCallCount { get; private set; }
        public int CommitCallCount { get; private set; }
        public int RollbackCallCount { get; private set; }
        public int BeginCallCount { get; private set; }
        public bool ThrowOnSave { get; set; }

        public FakeUnitOfWork(IUsuarioRepository usuarioRepository)
        {
            Usuarios = usuarioRepository;
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginCallCount++;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitCallCount++;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackCallCount++;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (ThrowOnSave)
            {
                throw new InvalidOperationException("Error de persistencia simulado.");
            }
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed_{password}";

        public bool VerifyPassword(string hashedPassword, string providedPassword) =>
            hashedPassword == $"hashed_{providedPassword}";
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

        public Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.Any(m => string.Equals(m.Dni, dni, StringComparison.OrdinalIgnoreCase)));
    }

    #endregion
}
