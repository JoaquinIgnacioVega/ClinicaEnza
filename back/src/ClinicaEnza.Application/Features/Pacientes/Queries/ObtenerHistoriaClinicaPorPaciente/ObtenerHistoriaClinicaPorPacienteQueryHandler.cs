using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerHistoriaClinicaPorPaciente;

public class ObtenerHistoriaClinicaPorPacienteQueryHandler
    : IRequestHandler<ObtenerHistoriaClinicaPorPacienteQuery, Result<HistoriaClinicaDto>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public ObtenerHistoriaClinicaPorPacienteQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    public async Task<Result<HistoriaClinicaDto>> Handle(
        ObtenerHistoriaClinicaPorPacienteQuery request,
        CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.ObtenerConHistoriaClinicaAsync(
            request.IdPaciente, cancellationToken);

        if (paciente == null)
            return Result.Failure<HistoriaClinicaDto>($"No se encontró el paciente con ID {request.IdPaciente}.");

        if (paciente.HistoriaClinica == null)
        {
            return Result.Failure<HistoriaClinicaDto>(
                $"No se encontró la historia clínica del paciente con ID {request.IdPaciente}.");
        }

        var prescripciones = paciente.HistoriaClinica.Prescripciones
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

        return Result.Success(new HistoriaClinicaDto
        {
            IdHistoriaClinica = paciente.HistoriaClinica.IdHistoriaClinica,
            IdPaciente = paciente.IdPersona,
            NombrePaciente = paciente.ObtenerNombreCompleto(),
            DniPaciente = paciente.Dni,
            MailPaciente = paciente.Mail,
            ObraSocial = paciente.ObraSocial,
            FechaNacimiento = paciente.FechaNacimiento,
            EdadPaciente = paciente.CalcularEdad(),
            Peso = paciente.Peso,
            Altura = paciente.Altura,
            MedicamentosRecetados = paciente.HistoriaClinica.ObtenerMedicamentosRecetados(),
            Prescripciones = prescripciones
        });
    }
}
