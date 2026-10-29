using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;

public class ObtenerMedicosQueryHandler : IRequestHandler<ObtenerMedicosQuery, Result<IEnumerable<MedicoDto>>>
{
    private readonly IRepository<Medico> _medicoRepository;

    public ObtenerMedicosQueryHandler(IRepository<Medico> medicoRepository)
    {
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<IEnumerable<MedicoDto>>> Handle(ObtenerMedicosQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Medico> medicos;

        if (request.Especialidad.HasValue)
        {
            medicos = await _medicoRepository.FindAsync(m => m.Especialidad == request.Especialidad.Value, cancellationToken);
        }
        else
        {
            medicos = await _medicoRepository.GetAllAsync(cancellationToken);
        }

        var dtos = medicos.Select(m => new MedicoDto
        {
            IdMedico = m.IdPersona,
            Nombre = m.Nombre,
            Apellido = m.Apellido,
            Dni = m.Dni,
            FechaNacimiento = m.FechaNacimiento,
            Mail = m.Mail,
            Matricula = m.Matricula,
            Especialidad = m.Especialidad,
            FechaInicio = m.FechaInicio,
            FichaProfesional = m.ObtenerFichaProfesional()
        }).ToList();

        return Result.Success<IEnumerable<MedicoDto>>(dtos);
    }
}
