using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Usuarios.Commands.DarDeBajaUsuario;
using ClinicaEnza.Application.Features.Usuarios.Commands.ModificarUsuario;
using ClinicaEnza.Application.Features.Usuarios.Commands.RegistrarUsuario;
using ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;
using ClinicaEnza.Application.Features.Usuarios.Queries.ObtenerUsuarios;
using ClinicaEnza.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerUsuarios(
        [FromQuery] bool? soloActivos,
        [FromQuery] RolUsuario? rol,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerUsuariosQuery(soloActivos, rol), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerUsuarioPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerUsuarioPorIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.Contains("No se encontró"))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegistrarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/usuarios/{result.Value.IdUsuario}", result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ModificarUsuario(int id, [FromBody] ModificarUsuarioCommand command, CancellationToken cancellationToken)
    {
        command.IdUsuario = id;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.Contains("No se encontró"))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [HttpPut("{id:int}/baja")]
    public async Task<IActionResult> DarDeBajaUsuario(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new DarDeBajaUsuarioCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error != null && result.Error.Contains("No se encontró"))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}

