using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.HorariosAtencion.Commands.AsignarConsultorioAHorario;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/horarios")]
public class HorariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public HorariosController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPut("{idHorario:int}/consultorio")]
    public async Task<IActionResult> AsignarConsultorio(
        int idHorario,
        [FromBody] AsignarConsultorioAHorarioCommand command,
        CancellationToken cancellationToken)
    {
        command.IdHorarioAtencion = idHorario;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
