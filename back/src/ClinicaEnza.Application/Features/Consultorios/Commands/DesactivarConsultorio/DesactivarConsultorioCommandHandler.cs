using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;
using ClinicaEnza.Domain;
namespace ClinicaEnza.Application.Features.Consultorios.Commands.DesactivarConsultorio;
public class DesactivarConsultorioCommandHandler : IRequestHandler<DesactivarConsultorioCommand, Result<ConsultorioDto>>
{
    private readonly IRepository<Consultorio> _repository; private readonly IUnitOfWork _unitOfWork;
    public DesactivarConsultorioCommandHandler(IRepository<Consultorio> repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task<Result<ConsultorioDto>> Handle(DesactivarConsultorioCommand request, CancellationToken cancellationToken = default)
    {
        var consultorio = await _repository.GetByIdAsync(request.IdConsultorio, cancellationToken);
        if (consultorio == null || !consultorio.Activo) return Result.Failure<ConsultorioDto>("No se encontró el consultorio activo.");
        consultorio.Desactivar(); _repository.Update(consultorio); await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new ConsultorioDto { IdConsultorio = consultorio.IdConsultorio, Numero = consultorio.Numero, Piso = consultorio.Piso, Sector = consultorio.Sector, Ubicacion = consultorio.ObtenerUbicacion(), Activo = consultorio.Activo });
    }
}
