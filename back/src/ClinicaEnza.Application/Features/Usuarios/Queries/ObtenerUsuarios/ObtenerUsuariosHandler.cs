using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarios;

public class ObtenerUsuariosHandler : IRequestHandler<ObtenerUsuariosQuery, Result<IReadOnlyList<UsuarioDto>>>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerUsuariosHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    }

    public async Task<Result<IReadOnlyList<UsuarioDto>>> Handle(ObtenerUsuariosQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Usuario> usuarios;

        if (request.SoloActivos.HasValue && request.Rol.HasValue)
        {
            usuarios = await _usuarioRepository.FindAsync(
                u => u.Activo == request.SoloActivos.Value && u.Rol == request.Rol.Value,
                cancellationToken);
        }
        else if (request.SoloActivos.HasValue)
        {
            usuarios = await _usuarioRepository.FindAsync(
                u => u.Activo == request.SoloActivos.Value,
                cancellationToken);
        }
        else if (request.Rol.HasValue)
        {
            usuarios = await _usuarioRepository.FindAsync(
                u => u.Rol == request.Rol.Value,
                cancellationToken);
        }
        else
        {
            usuarios = await _usuarioRepository.GetAllAsync(cancellationToken);
        }

        var dtos = usuarios.Select(u => new UsuarioDto
        {
            IdUsuario = u.IdPersona,
            IdPersona = u.IdPersona,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Dni = u.Dni,
            FechaNacimiento = u.FechaNacimiento,
            Mail = u.Mail,
            Legajo = u.Legajo,
            FechaInicio = u.FechaInicio,
            Rol = u.Rol,
            Activo = u.Activo,
            IdMedico = u.IdMedico
        }).ToList();

        return Result.Success<IReadOnlyList<UsuarioDto>>(dtos);
    }
}

public class ObtenerUsuariosQueryHandler : ObtenerUsuariosHandler
{
    public ObtenerUsuariosQueryHandler(IUsuarioRepository usuarioRepository) : base(usuarioRepository)
    {
    }
}

