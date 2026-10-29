using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Consultorios.Queries.ObtenerConsultorios;

public class ObtenerConsultoriosQueryHandler : IRequestHandler<ObtenerConsultoriosQuery, Result<IReadOnlyList<ConsultorioDto>>>
{
    private readonly IRepository<Consultorio> _consultorioRepository;

    public ObtenerConsultoriosQueryHandler(IRepository<Consultorio> consultorioRepository)
    {
        _consultorioRepository = consultorioRepository ?? throw new ArgumentNullException(nameof(consultorioRepository));
    }

    public async Task<Result<IReadOnlyList<ConsultorioDto>>> Handle(
        ObtenerConsultoriosQuery request,
        CancellationToken cancellationToken = default)
    {
        var consultorios = await _consultorioRepository.GetAllAsync(cancellationToken);

        var dtos = consultorios
            .OrderBy(c => c.Piso)
            .ThenBy(c => c.Numero)
            .Select(c => new ConsultorioDto
            {
                IdConsultorio = c.IdConsultorio,
                Numero = c.Numero,
                Piso = c.Piso,
                Sector = c.Sector,
                Ubicacion = c.ObtenerUbicacion()
            })
            .ToList();

        return Result.Success<IReadOnlyList<ConsultorioDto>>(dtos);
    }
}
