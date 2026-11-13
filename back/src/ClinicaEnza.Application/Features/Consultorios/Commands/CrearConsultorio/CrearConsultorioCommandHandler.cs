using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.Consultorios.DTOs;
using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Features.Consultorios.Commands.CrearConsultorio;

public class CrearConsultorioCommandHandler : IRequestHandler<CrearConsultorioCommand, Result<ConsultorioDto>>
{
    private readonly IRepository<Consultorio> _consultorioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearConsultorioCommandHandler(IRepository<Consultorio> consultorioRepository, IUnitOfWork unitOfWork)
    {
        _consultorioRepository = consultorioRepository ?? throw new ArgumentNullException(nameof(consultorioRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<ConsultorioDto>> Handle(CrearConsultorioCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Numero))
            return Result.Failure<ConsultorioDto>("El número de consultorio es obligatorio.");
        if (request.Numero.Trim().Length > 20)
            return Result.Failure<ConsultorioDto>("El número de consultorio no puede superar los 20 caracteres.");
        if (request.Piso < 0)
            return Result.Failure<ConsultorioDto>("El piso no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(request.Sector))
            return Result.Failure<ConsultorioDto>("El sector es obligatorio.");
        if (request.Sector.Trim().Length > 50)
            return Result.Failure<ConsultorioDto>("El sector no puede superar los 50 caracteres.");

        var numero = request.Numero.Trim();
        var sector = request.Sector.Trim();
        var existente = await _consultorioRepository.FindAsync(c => c.Numero == numero, cancellationToken);
        if (existente.Count > 0)
            return Result.Failure<ConsultorioDto>($"Ya existe un consultorio con el número {numero}.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var consultorio = new Consultorio(numero, request.Piso, sector);
            await _consultorioRepository.AddAsync(consultorio, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new ConsultorioDto
            {
                IdConsultorio = consultorio.IdConsultorio,
                Numero = consultorio.Numero,
                Piso = consultorio.Piso,
                Sector = consultorio.Sector,
                Ubicacion = consultorio.ObtenerUbicacion(),
                Activo = consultorio.Activo
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
