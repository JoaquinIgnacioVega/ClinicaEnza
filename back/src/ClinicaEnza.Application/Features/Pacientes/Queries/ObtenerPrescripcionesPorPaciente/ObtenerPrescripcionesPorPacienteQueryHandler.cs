using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPrescripcionesPorPaciente;

public class ObtenerPrescripcionesPorPacienteQueryHandler : IRequestHandler<ObtenerPrescripcionesPorPacienteQuery, Result<IEnumerable<PrescripcionMedicaDto>>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public ObtenerPrescripcionesPorPacienteQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
    }

    public async Task<Result<IEnumerable<PrescripcionMedicaDto>>> Handle(
        ObtenerPrescripcionesPorPacienteQuery request,
        CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.ObtenerConHistoriaClinicaAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
        {
            return Result.Failure<IEnumerable<PrescripcionMedicaDto>>($"No se encontró el paciente con ID {request.IdPaciente}.");
        }

        if (paciente.HistoriaClinica == null || paciente.HistoriaClinica.Prescripciones.Count == 0)
        {
            return Result.Success(Enumerable.Empty<PrescripcionMedicaDto>());
        }

        var dtos = paciente.HistoriaClinica.Prescripciones
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.IdPrescripcion)
            .Select(p => new PrescripcionMedicaDto
            {
                IdPrescripcion = p.IdPrescripcion,
                IdHistoriaClinica = paciente.HistoriaClinica.IdHistoriaClinica,
                IdPaciente = paciente.IdPersona,
                Fecha = p.Fecha,
                Medicamento = p.Medicamento,
                IdMedico = p.IdMedico,
                NombreMedico = p.Medico?.ObtenerNombreCompleto() ?? string.Empty,
                MatriculaMedico = p.Medico?.Matricula ?? string.Empty
            })
            .ToList();

        return Result.Success<IEnumerable<PrescripcionMedicaDto>>(dtos);
    }
}
