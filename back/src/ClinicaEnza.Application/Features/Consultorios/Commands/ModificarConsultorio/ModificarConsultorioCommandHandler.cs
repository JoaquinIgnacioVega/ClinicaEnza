using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Consultorios.Commands.ModificarConsultorio;
public class ModificarConsultorioCommandHandler : IRequestHandler<ModificarConsultorioCommand, Result<ConsultorioDto>>
{
    private readonly IRepository<Consultorio> _repository; private readonly IUnitOfWork _unitOfWork;
    public ModificarConsultorioCommandHandler(IRepository<Consultorio> repository, IUnitOfWork unitOfWork) { _repository = repository; _unitOfWork = unitOfWork; }
    public async Task<Result<ConsultorioDto>> Handle(ModificarConsultorioCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Numero)) return Result.Failure<ConsultorioDto>("El número de consultorio es obligatorio.");
        if (request.Piso < 0) return Result.Failure<ConsultorioDto>("El piso no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(request.Sector)) return Result.Failure<ConsultorioDto>("El sector es obligatorio.");
        var consultorio = await _repository.GetByIdAsync(request.IdConsultorio, cancellationToken);
        if (consultorio == null || !consultorio.Activo) return Result.Failure<ConsultorioDto>("No se encontró el consultorio.");
        var duplicado = await _repository.FindAsync(c => c.Activo && c.Numero == request.Numero.Trim() && c.IdConsultorio != request.IdConsultorio, cancellationToken);
        if (duplicado.Count > 0) return Result.Failure<ConsultorioDto>("Ya existe un consultorio con ese número.");
        consultorio.Modificar(request.Numero.Trim(), request.Piso, request.Sector.Trim()); _repository.Update(consultorio); await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new ConsultorioDto { IdConsultorio = consultorio.IdConsultorio, Numero = consultorio.Numero, Piso = consultorio.Piso, Sector = consultorio.Sector, Ubicacion = consultorio.ObtenerUbicacion(), Activo = consultorio.Activo });
    }
}
