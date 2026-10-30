using System.Collections.Generic;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarios;

public record ObtenerUsuariosQuery(bool? SoloActivos = null, RolUsuario? Rol = null) : IRequest<Result<IReadOnlyList<UsuarioDto>>>;

