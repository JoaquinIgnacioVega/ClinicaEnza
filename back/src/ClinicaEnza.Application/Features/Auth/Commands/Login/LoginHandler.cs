using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Interfaces.Services;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Auth.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
        : this(unitOfWork?.Usuarios ?? throw new ArgumentNullException(nameof(unitOfWork)), passwordHasher, jwtTokenGenerator)
    {
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Identificador))
            return Result.Failure<AuthResponseDto>("El identificador es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Failure<AuthResponseDto>("La contraseña es obligatoria.");

        var identificador = request.Identificador.Trim();

        // 1. Buscar usuario por Mail o por Legajo
        Usuario? usuario = null;
        if (identificador.Contains('@'))
        {
            usuario = await _usuarioRepository.ObtenerPorMailAsync(identificador, cancellationToken);
        }
        else
        {
            usuario = await _usuarioRepository.ObtenerPorLegajoAsync(identificador, cancellationToken);
            if (usuario == null)
            {
                usuario = await _usuarioRepository.ObtenerPorMailAsync(identificador, cancellationToken);
            }
        }

        // 2. Verificar que el usuario exista
        if (usuario == null)
        {
            return Result.Failure<AuthResponseDto>("Credenciales inválidas.");
        }

        // 3. Verificar que el usuario se encuentre activo
        if (!usuario.Activo)
        {
            return Result.Failure<AuthResponseDto>("El usuario no se encuentra activo.");
        }

        // 4. Verificar que la contraseña coincida
        var passwordValido = _passwordHasher.VerifyPassword(usuario.PasswordHash, request.Password);
        if (!passwordValido)
        {
            return Result.Failure<AuthResponseDto>("Credenciales inválidas.");
        }

        // 5. Generar token JWT
        var token = _jwtTokenGenerator.GenerateToken(usuario);

        // 6. Retornar DTO con token, nombre completo, rol e IdMedico si aplica
        var response = new AuthResponseDto
        {
            Token = token,
            NombreCompleto = usuario.ObtenerNombreCompleto(),
            Rol = usuario.Rol,
            IdMedico = usuario.IdMedico
        };

        return Result.Success(response);
    }
}

public class LoginCommandHandler : LoginHandler
{
    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
        : base(usuarioRepository, passwordHasher, jwtTokenGenerator)
    {
    }

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
        : base(unitOfWork, passwordHasher, jwtTokenGenerator)
    {
    }
}

