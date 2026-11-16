using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Pacientes.Commands.CambiarPasswordPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.AutenticarPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.CrearPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.ModificarPaciente;
using ClinicaEnza.Application.Features.Pacientes.Commands.RegistrarPrescripcion;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientePorId;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerHistoriaClinicaPorPaciente;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPrescripcionesPorPaciente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PacientesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [AllowAnonymous]
    [HttpPost("autenticar")]
    public async Task<IActionResult> Autenticar(
        [FromBody] AutenticarPacienteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPut("{idPaciente:int}/password")]
    public async Task<IActionResult> CambiarPassword(
        int idPaciente,
        [FromBody] CambiarPasswordPacienteCommand command,
        CancellationToken cancellationToken)
    {
        var claimId = User.FindFirst("idPaciente")?.Value;
        if (!int.TryParse(claimId, out var pacienteAutenticado) || pacienteAutenticado != idPaciente)
            return Forbid();

        var result = await _mediator.SendAsync(
            command with { IdPaciente = idPaciente }, cancellationToken);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [Authorize(Roles = "Administrador,Recepcionista,Medico")]
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

    [Authorize(Roles = "Administrador,Recepcionista,Medico,Paciente")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var esPersonalAutorizado = User.IsInRole("Administrador")
                                   || User.IsInRole("Recepcionista")
                                   || User.IsInRole("Medico");

        if (!esPersonalAutorizado)
        {
            var claimId = User.FindFirst("idPaciente")?.Value ?? User.FindFirst("IdPaciente")?.Value;
            if (!int.TryParse(claimId, out var pacienteAutenticado) || pacienteAutenticado != id)
            {
                return Forbid();
            }
        }

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

    [Authorize(Roles = "Administrador,Recepcionista")]
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

    [Authorize(Roles = "Administrador,Recepcionista")]
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

    [Authorize(Roles = "Medico")]
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

    [Authorize(Roles = "Medico,Paciente")]
    [HttpGet("{idPaciente:int}/prescripciones")]
    public async Task<IActionResult> ObtenerPrescripciones(
        int idPaciente,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Medico"))
        {
            var claimId = User.FindFirst("idPaciente")?.Value ?? User.FindFirst("IdPaciente")?.Value;
            if (!int.TryParse(claimId, out var pacienteAutenticado) || pacienteAutenticado != idPaciente)
            {
                return Forbid();
            }
        }

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

    [Authorize(Roles = "Medico,Paciente")]
    [HttpGet("{idPaciente:int}/historia-clinica")]
    public async Task<IActionResult> ObtenerHistoriaClinica(
        int idPaciente,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Medico"))
        {
            var claimId = User.FindFirst("idPaciente")?.Value ?? User.FindFirst("IdPaciente")?.Value;
            if (!int.TryParse(claimId, out var pacienteAutenticado) || pacienteAutenticado != idPaciente)
            {
                return Forbid();
            }
        }

        var result = await _mediator.SendAsync(
            new ObtenerHistoriaClinicaPorPacienteQuery(idPaciente), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

