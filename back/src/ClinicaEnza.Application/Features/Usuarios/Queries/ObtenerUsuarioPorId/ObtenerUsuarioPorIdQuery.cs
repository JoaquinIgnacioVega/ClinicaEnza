using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Usuarios.DTOs;

namespace ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;

public record ObtenerUsuarioPorIdQuery(int IdUsuario) : IRequest<Result<UsuarioDto>>;

