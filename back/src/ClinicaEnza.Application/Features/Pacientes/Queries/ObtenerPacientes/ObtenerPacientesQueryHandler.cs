using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Pacientes.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerPacientes;

public class ObtenerPacientesQueryHandler : IRequestHandler<ObtenerPacientesQuery, Result<IReadOnlyList<PacienteDto>>>
{
    private readonly IPacienteRepository _pacienteRepository;

    public ObtenerPacientesQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
    }

    public async Task<Result<IReadOnlyList<PacienteDto>>> Handle(ObtenerPacientesQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Paciente> pacientes;

        if (!string.IsNullOrWhiteSpace(request.Filtro))
        {
            var filtro = request.Filtro.Trim().ToLower();
            pacientes = await _pacienteRepository.FindAsync(
                p => (p.Dni != null && p.Dni.ToLower().Contains(filtro)) ||
                     (p.Nombre != null && p.Nombre.ToLower().Contains(filtro)) ||
                     (p.Apellido != null && p.Apellido.ToLower().Contains(filtro)),
                cancellationToken);
        }
        else
        {
            pacientes = await _pacienteRepository.GetAllAsync(cancellationToken);
        }

        var dtos = pacientes.Select(p => new PacienteDto
        {
            IdPaciente = p.IdPersona,
            Nombre = p.Nombre,
            Apellido = p.Apellido,
            Dni = p.Dni,
            FechaNacimiento = p.FechaNacimiento,
            Edad = p.CalcularEdad(),
            Mail = p.Mail,
            ObraSocial = p.ObraSocial,
            Peso = p.Peso,
            Altura = p.Altura,
            IdHistoriaClinica = p.HistoriaClinica?.IdHistoriaClinica
        }).ToList();

        return Result.Success<IReadOnlyList<PacienteDto>>(dtos);
    }
}

