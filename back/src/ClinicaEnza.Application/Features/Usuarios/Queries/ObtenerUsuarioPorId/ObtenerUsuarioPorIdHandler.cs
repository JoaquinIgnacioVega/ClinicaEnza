using System;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;

namespace ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;

public class ObtenerUsuarioPorIdHandler : IRequestHandler<ObtenerUsuarioPorIdQuery, Result<UsuarioDto>>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerUsuarioPorIdHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    }

    public async Task<Result<UsuarioDto>> Handle(ObtenerUsuarioPorIdQuery request, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(request.IdUsuario, cancellationToken);
        if (usuario == null)
        {
            return Result.Failure<UsuarioDto>($"No se encontró el usuario con ID {request.IdUsuario}.");
        }

        var dto = new UsuarioDto
        {
            IdUsuario = usuario.IdPersona,
            IdPersona = usuario.IdPersona,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Dni = usuario.Dni,
            FechaNacimiento = usuario.FechaNacimiento,
            Mail = usuario.Mail,
            Legajo = usuario.Legajo,
            FechaInicio = usuario.FechaInicio,
            Rol = usuario.Rol,
            Activo = usuario.Activo,
            IdMedico = usuario.IdMedico
        };

        return Result.Success(dto);
    }
}

public class ObtenerUsuarioPorIdQueryHandler : ObtenerUsuarioPorIdHandler
{
    public ObtenerUsuarioPorIdQueryHandler(IUsuarioRepository usuarioRepository) : base(usuarioRepository)
    {
    }
}

