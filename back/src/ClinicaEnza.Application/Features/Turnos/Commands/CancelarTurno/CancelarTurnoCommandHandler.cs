using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Features.Turnos.Commands.CancelarTurno;

public class CancelarTurnoCommandHandler : IRequestHandler<CancelarTurnoCommand, Result>
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelarTurnoCommandHandler(ITurnoRepository turnoRepository, IUnitOfWork unitOfWork)
    {
        _turnoRepository = turnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelarTurnoCommand request, CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.GetByIdAsync(request.IdTurno, cancellationToken);
        if (turno == null)
            return Result.Failure($"No se encontró el turno con ID {request.IdTurno}.");

        turno.CambiarEstado(EstadoTurno.Cancelado);
        _turnoRepository.Update(turno);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

