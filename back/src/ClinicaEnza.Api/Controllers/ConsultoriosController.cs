using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Consultorios.Commands.CrearConsultorio;
using ClinicaEnza.Application.Features.Consultorios.Commands.ModificarConsultorio;
using ClinicaEnza.Application.Features.Consultorios.Commands.DesactivarConsultorio;
using ClinicaEnza.Application.Features.Consultorios.Queries.ObtenerConsultorios;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Recepcionista")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearConsultorioCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Created($"/api/consultorios/{result.Value.IdConsultorio}", result.Value);
    }

    [Authorize(Roles = "Recepcionista")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarConsultorioCommand command, CancellationToken cancellationToken)
    {
        command.IdConsultorio = id;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Authorize(Roles = "Recepcionista")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new DesactivarConsultorioCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound(new { error = result.Error });
        return NoContent();
    }
}
