using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Medicos.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicoPorId;

public class ObtenerMedicoPorIdQueryHandler : IRequestHandler<ObtenerMedicoPorIdQuery, Result<MedicoDto>>
{
    private readonly IRepository<Medico> _medicoRepository;

    public ObtenerMedicoPorIdQueryHandler(IRepository<Medico> medicoRepository)
    {
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
    }

    public async Task<Result<MedicoDto>> Handle(ObtenerMedicoPorIdQuery request, CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
        {
            return Result.Failure<MedicoDto>($"No se encontró el médico con ID {request.IdMedico}.");
        }

        var dto = new MedicoDto
        {
            IdMedico = medico.IdPersona,
            Nombre = medico.Nombre,
            Apellido = medico.Apellido,
            Dni = medico.Dni,
            FechaNacimiento = medico.FechaNacimiento,
            Mail = medico.Mail,
            Matricula = medico.Matricula,
            Especialidad = medico.Especialidad,
            FechaInicio = medico.FechaInicio,
            FichaProfesional = medico.ObtenerFichaProfesional()
        };

        return Result.Success(dto);
    }
}
