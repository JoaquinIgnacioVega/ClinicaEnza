using System;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientePorId;

public class ObtenerPacientePorIdQueryHandler : IRequestHandler<ObtenerPacientePorIdQuery, Result<PacienteDto>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public ObtenerPacientePorIdQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
    }

    public async Task<Result<PacienteDto>> Handle(ObtenerPacientePorIdQuery request, CancellationToken cancellationToken = default)
    {
        if (request.IdPaciente <= 0)
        {
            return Result.Failure<PacienteDto>("El ID del paciente debe ser mayor a cero.");
        }

        var paciente = await _pacienteRepository.GetByIdAsync(request.IdPaciente, cancellationToken);
        if (paciente == null)
        {
            return Result.Failure<PacienteDto>($"No se encontró el paciente con ID {request.IdPaciente}.");
        }

        var dto = new PacienteDto
        {
            IdPaciente = paciente.IdPersona,
            Nombre = paciente.Nombre,
            Apellido = paciente.Apellido,
            Dni = paciente.Dni,
            FechaNacimiento = paciente.FechaNacimiento,
            Edad = paciente.CalcularEdad(),
            Mail = paciente.Mail,
            ObraSocial = paciente.ObraSocial,
            Peso = paciente.Peso,
            Altura = paciente.Altura,
            IdHistoriaClinica = paciente.HistoriaClinica?.IdHistoriaClinica
        };

        return Result.Success(dto);
    }
}

