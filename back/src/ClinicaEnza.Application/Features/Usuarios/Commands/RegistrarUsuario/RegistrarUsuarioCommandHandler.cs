using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.RegistrarUsuario;

public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<Medico> _medicoRepository;

    public RegistrarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IRepository<Medico> medicoRepository)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<UsuarioDto>> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas de entrada
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<UsuarioDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<UsuarioDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Dni))
            return Result.Failure<UsuarioDto>("El DNI es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<UsuarioDto>("El correo electrónico es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Legajo))
            return Result.Failure<UsuarioDto>("El legajo es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<UsuarioDto>("La contraseña es obligatoria.");

        // 2. Validar que el DNI no esté duplicado
        if (await _usuarioRepository.ExisteDniAsync(request.Dni.Trim(), cancellationToken))
            return Result.Failure<UsuarioDto>($"Ya existe una persona registrada con el DNI {request.Dni}.");

        // 3. Validar que el Mail no esté duplicado
        if (await _usuarioRepository.ExisteMailAsync(request.Mail.Trim(), cancellationToken))
            return Result.Failure<UsuarioDto>($"Ya existe una persona registrada con el correo electrónico {request.Mail}.");

        // 4. Validar que el Legajo no esté duplicado
        if (await _usuarioRepository.ExisteLegajoAsync(request.Legajo.Trim(), cancellationToken))
            return Result.Failure<UsuarioDto>($"Ya existe un usuario registrado con el legajo {request.Legajo}.");

        // 5. Validar coherencia del rol y médico
        Medico? medico = null;
        if (request.Rol == RolUsuario.Medico)
        {
            if (!request.IdMedico.HasValue)
                return Result.Failure<UsuarioDto>("Un usuario con rol Médico debe especificar el Id del médico asociado.");

            medico = await _medicoRepository.GetByIdAsync(request.IdMedico.Value, cancellationToken);
            if (medico == null)
                return Result.Failure<UsuarioDto>($"No se encontró el médico con ID {request.IdMedico.Value}.");
        }
        else if (request.IdMedico.HasValue)
        {
            medico = await _medicoRepository.GetByIdAsync(request.IdMedico.Value, cancellationToken);
            if (medico == null)
                return Result.Failure<UsuarioDto>($"No se encontró el médico con ID {request.IdMedico.Value}.");
        }

        // 6. Hashear la contraseña
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 7. Iniciar transacción y persistir
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var usuario = new Usuario(
                request.Nombre.Trim(),
                request.Apellido.Trim(),
                request.Dni.Trim(),
                request.FechaNacimiento,
                request.Mail.Trim(),
                request.Legajo.Trim(),
                passwordHash,
                request.Rol,
                request.IdMedico,
                medico);

            await _usuarioRepository.AddAsync(usuario, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var usuarioDto = new UsuarioDto
            {
                IdUsuario = usuario.IdPersona,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Dni = usuario.Dni,
                FechaNacimiento = usuario.FechaNacimiento,
                Mail = usuario.Mail,
                Legajo = usuario.Legajo,
                FechaInicio = usuario.FechaInicio,
                Rol = usuario.Rol,
                IdMedico = usuario.IdMedico
            };

            return Result.Success(usuarioDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

