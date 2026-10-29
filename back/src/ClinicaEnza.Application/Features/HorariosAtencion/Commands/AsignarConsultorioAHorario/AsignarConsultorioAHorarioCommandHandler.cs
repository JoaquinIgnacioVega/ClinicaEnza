using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Commands.AsignarConsultorioAHorario;

public class AsignarConsultorioAHorarioCommandHandler : IRequestHandler<AsignarConsultorioAHorarioCommand, Result<HorarioAtencionDto>>
{
    private readonly IRepository<HorarioAtencion> _horarioRepository;
    private readonly IRepository<Consultorio> _consultorioRepository;
    private readonly IRepository<Medico> _medicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AsignarConsultorioAHorarioCommandHandler(
        IRepository<HorarioAtencion> horarioRepository,
        IRepository<Consultorio> consultorioRepository,
        IRepository<Medico> medicoRepository,
        IUnitOfWork unitOfWork)
    {
        _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
        _consultorioRepository = consultorioRepository ?? throw new ArgumentNullException(nameof(consultorioRepository));
        _medicoRepository = medicoRepository ?? throw new ArgumentNullException(nameof(medicoRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<HorarioAtencionDto>> Handle(
        AsignarConsultorioAHorarioCommand request,
        CancellationToken cancellationToken = default)
    {
        var horario = await _horarioRepository.GetByIdAsync(request.IdHorarioAtencion, cancellationToken);
        if (horario == null)
            return Result.Failure<HorarioAtencionDto>($"No se encontró el horario de atención con ID {request.IdHorarioAtencion}.");

        var consultorio = await _consultorioRepository.GetByIdAsync(request.IdConsultorio, cancellationToken);
        if (consultorio == null)
            return Result.Failure<HorarioAtencionDto>($"No se encontró el consultorio con ID {request.IdConsultorio}.");

        // Validar que el consultorio no esté ocupado por otro médico en esa franja horaria y día
        var horariosDelDiaEnConsultorio = await _horarioRepository.FindAsync(
            h => h.DiaSemana == horario.DiaSemana 
                && h.IdConsultorio == request.IdConsultorio 
                && h.IdHorarioAtencion != horario.IdHorarioAtencion,
            cancellationToken);

        if (horariosDelDiaEnConsultorio.Any(h => h.EsSolapadoCon(horario)))
        {
            throw new HorarioAtencionSuperpuestoException(
                $"El consultorio '{consultorio.Numero}' ya está asignado a otro médico en una franja superpuesta para {horario.DiaSemana}.");
        }

        var medico = horario.Medico ?? await _medicoRepository.GetByIdAsync(horario.IdMedico, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            horario.AsignarConsultorio(consultorio);
            _horarioRepository.Update(horario);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new HorarioAtencionDto
            {
                IdHorarioAtencion = horario.IdHorarioAtencion,
                IdMedico = horario.IdMedico,
                NombreMedico = medico?.ObtenerNombreCompleto() ?? string.Empty,
                DiaSemana = horario.DiaSemana,
                HoraDesde = horario.HoraDesde,
                HoraHasta = horario.HoraHasta,
                IdConsultorio = consultorio.IdConsultorio,
                NumeroConsultorio = consultorio.Numero
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
