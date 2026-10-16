using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Common.Mediator;
using ClinicaEnza.Application.Common.Models;
using ClinicaEnza.Application.Features.HorariosAtencion.DTOs;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Application.Features.HorariosAtencion.Commands.ConfigurarHorarioAtencion;

public class ConfigurarHorarioAtencionCommandHandler : IRequestHandler<ConfigurarHorarioAtencionCommand, Result<HorarioAtencionDto>>
{
    private readonly IRepository<HorarioAtencion> _horarioRepository;
    private readonly IRepository<Medico> _medicoRepository;
    private readonly IRepository<Consultorio> _consultorioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigurarHorarioAtencionCommandHandler(
        IRepository<HorarioAtencion> horarioRepository,
        IRepository<Medico> medicoRepository,
        IRepository<Consultorio> consultorioRepository,
        IUnitOfWork unitOfWork)
    {
        _horarioRepository = horarioRepository;
        _medicoRepository = medicoRepository;
        _consultorioRepository = consultorioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HorarioAtencionDto>> Handle(
        ConfigurarHorarioAtencionCommand request,
        CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.GetByIdAsync(request.IdMedico, cancellationToken);
        if (medico == null)
            return Result.Failure<HorarioAtencionDto>($"No se encontró el médico con ID {request.IdMedico}.");

        var consultorio = await _consultorioRepository.GetByIdAsync(request.IdConsultorio, cancellationToken);
        if (consultorio == null)
            return Result.Failure<HorarioAtencionDto>($"No se encontró el consultorio con ID {request.IdConsultorio}.");

        var horariosDelDia = await _horarioRepository.FindAsync(
            h => h.DiaSemana == request.DiaSemana,
            cancellationToken);

        var horarioNuevo = new HorarioAtencion(
            request.DiaSemana,
            request.HoraDesde,
            request.HoraHasta,
            medico,
            consultorio);

        if (horariosDelDia.Any(h => h.IdMedico == request.IdMedico && h.EsSolapadoCon(horarioNuevo)))
        {
            throw new HorarioAtencionSuperpuestoException(
                $"El médico con ID {request.IdMedico} ya tiene un horario superpuesto para {request.DiaSemana}.");
        }

        if (horariosDelDia.Any(h => h.IdConsultorio == request.IdConsultorio && h.EsSolapadoCon(horarioNuevo)))
        {
            throw new HorarioAtencionSuperpuestoException(
                $"El consultorio '{consultorio.Numero}' ya está asignado a otro médico en una franja superpuesta para {request.DiaSemana}.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _horarioRepository.AddAsync(horarioNuevo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new HorarioAtencionDto
            {
                IdHorarioAtencion = horarioNuevo.IdHorarioAtencion,
                IdMedico = medico.IdPersona,
                NombreMedico = medico.ObtenerNombreCompleto(),
                DiaSemana = horarioNuevo.DiaSemana,
                HoraDesde = horarioNuevo.HoraDesde,
                HoraHasta = horarioNuevo.HoraHasta,
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
