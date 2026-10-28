using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.Commands.ModificarUsuario;

public class ModificarUsuarioCommandHandler : IRequestHandler<ModificarUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Medico> _medicoRepository;

    public ModificarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IRepository<Medico> medicoRepository)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<UsuarioDto>> Handle(ModificarUsuarioCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validaciones básicas de entrada
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure<UsuarioDto>("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            return Result.Failure<UsuarioDto>("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Mail))
            return Result.Failure<UsuarioDto>("El correo electrónico es obligatorio.");

        // 2. Buscar usuario existente
        var usuario = await _usuarioRepository.GetByIdAsync(request.IdUsuario, cancellationToken);
        if (usuario == null)
            return Result.Failure<UsuarioDto>($"No se encontró el usuario con ID {request.IdUsuario}.");

        // 3. Validar que el nuevo correo no pertenezca a otra persona
        if (await _usuarioRepository.ExisteMailParaOtraPersonaAsync(request.Mail.Trim(), request.IdUsuario, cancellationToken))
            return Result.Failure<UsuarioDto>($"El correo electrónico '{request.Mail}' ya se encuentra registrado por otra persona.");

        // 4. Validar rol y médico asociado
        if (request.Rol == RolUsuario.Medico)
        {
            if (!request.IdMedico.HasValue)
                return Result.Failure<UsuarioDto>("Un usuario con rol Médico debe especificar el Id del médico asociado.");

            var medico = await _medicoRepository.GetByIdAsync(request.IdMedico.Value, cancellationToken);
            if (medico == null)
                return Result.Failure<UsuarioDto>($"No se encontró el médico con ID {request.IdMedico.Value}.");
        }
        else if (request.IdMedico.HasValue)
        {
            var medico = await _medicoRepository.GetByIdAsync(request.IdMedico.Value, cancellationToken);
            if (medico == null)
                return Result.Failure<UsuarioDto>($"No se encontró el médico con ID {request.IdMedico.Value}.");
        }

        // 5. Invocar método de dominio para actualizar datos
        usuario.ActualizarDatos(
            request.Nombre.Trim(),
            request.Apellido.Trim(),
            request.Mail.Trim(),
            request.Rol,
            request.IdMedico);

        // 6. Persistir cambios
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Retornar DTO con los datos actualizados
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
}

