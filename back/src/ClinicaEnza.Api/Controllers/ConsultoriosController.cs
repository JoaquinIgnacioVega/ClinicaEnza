using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Consultorios.Queries.ObtenerConsultorios;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/consultorios")]
public class ConsultoriosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConsultoriosController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerConsultoriosQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
