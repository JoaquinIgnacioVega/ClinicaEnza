using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnoPorId;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurnosController : ControllerBase
{
    private readonly IMediator _mediator;

    public TurnosController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] CrearTurnoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Value.IdTurno }, result.Value);
    }

    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> CancelarTurno(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new CancelarTurnoCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerTurnoPorIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("medico/{idMedico:int}")]
    public async Task<IActionResult> ObtenerPorMedico(int idMedico, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerTurnosPorMedicoQuery(idMedico), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

