using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Usuarios.Commands.RegistrarUsuario;
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
}

