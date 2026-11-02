using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.ReagendarTurno;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnoPorId;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnos;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;
using ClinicaEnza.Domain.Enums;
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

    [HttpGet]
    public async Task<IActionResult> ObtenerTurnos(
        [FromQuery] DateTime? fecha,
        [FromQuery] int? idPaciente,
        [FromQuery] int? idMedico,
        [FromQuery] EstadoTurno? estado,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(
            new ObtenerTurnosQuery(fecha, idPaciente, idMedico, estado), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
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

    [HttpPut("{id:int}/reagendar")]
    public async Task<IActionResult> ReagendarTurno(
        int id,
        [FromBody] ReagendarTurnoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(
            new ReagendarTurnoCommand(id, request.NuevaHoraInicio), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
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
    public async Task<IActionResult> ObtenerPorMedico(
        int idMedico,
        [FromQuery] DateTime? fecha,
        [FromQuery] EstadoTurno? estado,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(
            new ObtenerTurnosPorMedicoQuery(idMedico, fecha, estado), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

public record ReagendarTurnoRequest(DateTime NuevaHoraInicio);

