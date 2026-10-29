using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Auth.Commands.Login;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return BadRequest(new { error = "El cuerpo de la solicitud no puede estar vacío." });
        }

        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

