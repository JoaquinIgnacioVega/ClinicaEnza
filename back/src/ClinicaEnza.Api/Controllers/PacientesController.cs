using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.ModificarPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientePorId;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPrescripcionesPorPaciente;
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
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] string? filtro,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerPacientesQuery(filtro), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerPacientePorIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
            {
                return NotFound(new { error = result.Error });
            }

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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(
        int id,
        [FromBody] ModificarPacienteCommand command,
        CancellationToken cancellationToken)
    {
        command.IdPaciente = id;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
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

    [HttpGet("{idPaciente:int}/prescripciones")]
    public async Task<IActionResult> ObtenerPrescripciones(
        int idPaciente,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerPrescripcionesPorPacienteQuery(idPaciente), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

