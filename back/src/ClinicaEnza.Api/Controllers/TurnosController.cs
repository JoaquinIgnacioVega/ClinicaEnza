using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurnoPaciente;
using ClinicaEnza.Application.Features.Turnos.Commands.CrearTurno;
using ClinicaEnza.Application.Features.Turnos.Commands.MarcarTurnoRealizado;
using ClinicaEnza.Application.Features.Turnos.Commands.ReagendarTurno;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerAgendaMedico;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnoPorId;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnos;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorMedico;
using ClinicaEnza.Application.Features.Turnos.Queries.ObtenerTurnosPorPaciente;
using ClinicaEnza.Domain.Enums;
using Microsoft.AspNetCore.Http;
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

    [HttpPut("{id:int}/realizar")]
    public async Task<IActionResult> MarcarTurnoRealizado(
        int id,
        [FromBody] MarcarTurnoRealizadoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(
            new MarcarTurnoRealizadoCommand(id, request.MedicamentoRecetado), cancellationToken);

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

    [HttpGet("/api/medicos/{idMedico:int}/turnos/agenda")]
    public async Task<IActionResult> ObtenerAgendaMedico(
        int idMedico,
        [FromQuery] DateTime? fecha,
        [FromQuery] EstadoTurno? estado,
        [FromQuery] int? idMedicoSolicitante = null,
        CancellationToken cancellationToken = default)
    {
        int? medicoSolicitante = null;

        var claimIdMedico = User.FindFirst("idMedico")?.Value ?? User.FindFirst("IdMedico")?.Value;
        if (!string.IsNullOrEmpty(claimIdMedico) && int.TryParse(claimIdMedico, out var parsedClaimId))
        {
            medicoSolicitante = parsedClaimId;
        }
        else if (Request.Headers.TryGetValue("X-Id-Medico", out var headerVal) && int.TryParse(headerVal, out var parsedHeaderId))
        {
            medicoSolicitante = parsedHeaderId;
        }
        else if (idMedicoSolicitante.HasValue)
        {
            medicoSolicitante = idMedicoSolicitante.Value;
        }

        if (!medicoSolicitante.HasValue)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "No tienes autorización para consultar la agenda de otro profesional."
            });
        }

        var result = await _mediator.SendAsync(
            new ObtenerAgendaMedicoQuery(idMedico, medicoSolicitante.Value, fecha, estado),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("autorización") == true)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error });
            }

            if (result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("/api/pacientes/{idPaciente:int}/turnos")]
    public async Task<IActionResult> ObtenerPorPaciente(
        int idPaciente,
        [FromQuery] bool soloPendientes = true,
        CancellationToken cancellationToken = default)
    {
        var claimId = User.FindFirst("idPaciente")?.Value ?? User.FindFirst("IdPaciente")?.Value;
        if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out var claimPacienteId))
        {
            if (claimPacienteId != idPaciente && !User.IsInRole("Administrador") && !User.IsInRole("Recepcionista"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permiso para acceder a los turnos de otro paciente." });
            }
        }

        var result = await _mediator.SendAsync(
            new ObtenerTurnosPorPacienteQuery(idPaciente, soloPendientes), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("/api/pacientes/{idPaciente:int}/turnos/{idTurno:int}/cancelar")]
    public async Task<IActionResult> CancelarPorPaciente(
        int idPaciente,
        int idTurno,
        CancellationToken cancellationToken = default)
    {
        var claimId = User.FindFirst("idPaciente")?.Value ?? User.FindFirst("IdPaciente")?.Value;
        int pacienteIdEfectivo = idPaciente;
        if (!string.IsNullOrEmpty(claimId) && int.TryParse(claimId, out var claimPacienteId))
        {
            if (claimPacienteId != idPaciente && !User.IsInRole("Administrador") && !User.IsInRole("Recepcionista"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "No tienes permiso para cancelar un turno que pertenece a otro paciente." });
            }
            pacienteIdEfectivo = claimPacienteId;
        }

        var result = await _mediator.SendAsync(
            new CancelarTurnoPacienteCommand(idTurno, pacienteIdEfectivo), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("No tienes permiso") == true)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error });
            }

            if (result.Error?.Contains("no existe") == true || result.Error?.Contains("No se encontró") == true)
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { mensaje = "Turno cancelado exitosamente.", turno = result.Value });
    }
}

public record ReagendarTurnoRequest(DateTime NuevaHoraInicio);
public record MarcarTurnoRealizadoRequest(string? MedicamentoRecetado);

