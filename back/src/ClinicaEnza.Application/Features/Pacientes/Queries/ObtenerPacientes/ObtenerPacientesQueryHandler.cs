using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;

public class ObtenerPacientesQueryHandler : IRequestHandler<ObtenerPacientesQuery, Result<IReadOnlyList<PacienteDto>>>
{
    private readonly IRepository<Paciente> _pacienteRepository;

    public ObtenerPacientesQueryHandler(IRepository<Paciente> pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    public async Task<Result<IReadOnlyList<PacienteDto>>> Handle(ObtenerPacientesQuery request, CancellationToken cancellationToken = default)
    {
        var pacientes = await _pacienteRepository.GetAllAsync(cancellationToken);

        var dtos = pacientes.Select(p => new PacienteDto
        {
            IdPaciente = p.IdPersona,
            Nombre = p.Nombre,
            Apellido = p.Apellido,
            Dni = p.Dni,
            FechaNacimiento = p.FechaNacimiento,
            Mail = p.Mail,
            ObraSocial = p.ObraSocial,
            Peso = p.Peso,
            Altura = p.Altura
        }).ToList();

        return Result.Success<IReadOnlyList<PacienteDto>>(dtos);
    }
}

