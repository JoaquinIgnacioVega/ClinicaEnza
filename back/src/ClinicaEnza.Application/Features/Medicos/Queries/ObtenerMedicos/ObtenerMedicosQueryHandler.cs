using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;

public class ObtenerMedicosQueryHandler : IRequestHandler<ObtenerMedicosQuery, Result<IReadOnlyList<MedicoDto>>>
{
    private readonly IRepository<Medico> _medicoRepository;

    public ObtenerMedicosQueryHandler(IRepository<Medico> medicoRepository)
    {
        _medicoRepository = medicoRepository;
    }

    public async Task<Result<IReadOnlyList<MedicoDto>>> Handle(ObtenerMedicosQuery request, CancellationToken cancellationToken = default)
    {
        var medicos = await _medicoRepository.GetAllAsync(cancellationToken);

        var dtos = medicos.Select(m => new MedicoDto
        {
            IdMedico = m.IdPersona,
            Nombre = m.Nombre,
            Apellido = m.Apellido,
            Matricula = m.Matricula,
            Especialidad = m.Especialidad,
            Mail = m.Mail
        }).ToList();

        return Result.Success<IReadOnlyList<MedicoDto>>(dtos);
    }
}

