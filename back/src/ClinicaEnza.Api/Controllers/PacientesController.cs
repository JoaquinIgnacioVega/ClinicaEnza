using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PacientesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerPacientesQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPacienteCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/pacientes/{result.Value.IdPaciente}", result.Value);
    }

    [HttpPost("{idPaciente:int}/prescripciones")]
    public async Task<IActionResult> RegistrarPrescripcion(
        int idPaciente,
        [FromBody] RegistrarPrescripcionCommand command,
        CancellationToken cancellationToken)
    {
        command.IdPaciente = idPaciente;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/pacientes/{idPaciente}/prescripciones/{result.Value.IdPrescripcion}", result.Value);
    }
}
