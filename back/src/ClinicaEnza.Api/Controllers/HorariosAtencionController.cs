using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;
using ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/horarios-atencion")]
public class HorariosAtencionController : ControllerBase
{
    private readonly IMediator _mediator;

    public HorariosAtencionController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    public async Task<IActionResult> Configurar(
        [FromBody] ConfigurarHorarioAtencionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Configurar), new { id = result.Value.IdHorarioAtencion }, result.Value);
    }

    [HttpGet("medico/{idMedico:int}")]
    public async Task<IActionResult> ObtenerPorMedico(
        int idMedico,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(
            new ObtenerHorariosPorMedicoQuery(idMedico),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
