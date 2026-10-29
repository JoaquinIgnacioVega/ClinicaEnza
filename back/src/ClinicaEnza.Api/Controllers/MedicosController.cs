using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;
using ClinicaEnza.Application.Features.HorariosAtencion.Queries.ObtenerHorariosPorMedico;
using ClinicaEnza.Application.Features.Medicos.Commands.ModificarMedico;
using ClinicaEnza.Application.Features.Medicos.Commands.RegistrarMedico;
using ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicoPorId;
using ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;
using ClinicaEnza.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaEnza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicosController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicosController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] Especialidad? especialidad,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerMedicosQuery(especialidad), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("especialidades")]
    public IActionResult ObtenerEspecialidades()
    {
        var especialidades = Enum.GetValues<Especialidad>()
            .Select(e => new
            {
                id = (int)e,
                nombre = e.ToString()
            });

        return Ok(especialidades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerMedicoPorIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarMedicoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Created($"/api/medicos/{result.Value.IdMedico}", result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(
        int id,
        [FromBody] ModificarMedicoCommand command,
        CancellationToken cancellationToken)
    {
        command.IdMedico = id;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("{idMedico:int}/horarios")]
    public async Task<IActionResult> ConfigurarHorario(
        int idMedico,
        [FromBody] ConfigurarHorarioAtencionCommand command,
        CancellationToken cancellationToken)
    {
        command.IdMedico = idMedico;
        var result = await _mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/medicos/{idMedico}/horarios/{result.Value.IdHorarioAtencion}", result.Value);
    }

    [HttpGet("{idMedico:int}/horarios")]
    public async Task<IActionResult> ObtenerHorariosPorMedico(
        int idMedico,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.SendAsync(new ObtenerHorariosPorMedicoQuery(idMedico), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("No se encontró") == true)
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
