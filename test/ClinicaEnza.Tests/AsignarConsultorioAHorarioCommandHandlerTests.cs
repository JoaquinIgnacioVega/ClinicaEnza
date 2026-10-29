using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Consultorios.Queries.ObtenerConsultorios;
using ClinicaEnza.Application.Features.HorariosAtencion.Commands.AsignarConsultorioAHorario;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using ClinicaEnza.Domain.Exceptions;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class AsignarConsultorioAHorarioCommandHandlerTests
{
    private readonly FakeRepository<HorarioAtencion> _horarioRepository;
    private readonly FakeRepository<Consultorio> _consultorioRepository;
    private readonly FakeRepository<Medico> _medicoRepository;
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly AsignarConsultorioAHorarioCommandHandler _handler;
    private readonly ObtenerConsultoriosQueryHandler _obtenerConsultoriosHandler;

    public AsignarConsultorioAHorarioCommandHandlerTests()
    {
        _horarioRepository = new FakeRepository<HorarioAtencion>();
        _consultorioRepository = new FakeRepository<Consultorio>();
        _medicoRepository = new FakeRepository<Medico>();
        _unitOfWork = new FakeUnitOfWork();

        _handler = new AsignarConsultorioAHorarioCommandHandler(
            _horarioRepository,
            _consultorioRepository,
            _medicoRepository,
            _unitOfWork);

        _obtenerConsultoriosHandler = new ObtenerConsultoriosQueryHandler(_consultorioRepository);
    }

    private Medico CrearMedico(int id, string matricula)
    {
        var medico = new Medico("Carlos", "Gomez", $"20111{id:D3}", new DateTime(1980, 5, 10), $"medico{id}@test.com", Especialidad.Cardiologia, matricula, new DateTime(2015, 1, 1))
        {
            IdPersona = id
        };
        _medicoRepository.Entities.Add(medico);
        return medico;
    }

    private Consultorio CrearConsultorio(int id, string numero, int piso = 1, string sector = "Ala Norte")
    {
        var consultorio = new Consultorio(numero, piso, sector)
        {
            IdConsultorio = id
        };
        _consultorioRepository.Entities.Add(consultorio);
        return consultorio;
    }

    private HorarioAtencion CrearHorario(int id, DayOfWeek diaSemana, TimeSpan desde, TimeSpan hasta, Medico medico, Consultorio consultorio)
    {
        var horario = new HorarioAtencion(diaSemana, desde, hasta, medico, consultorio)
        {
            IdHorarioAtencion = id
        };
        _horarioRepository.Entities.Add(horario);
        return horario;
    }

    [Fact]
    public async Task Handle_DatosValidos_AsignaConsultorioYConfirmaTransaccion()
    {
        // Arrange
        var medico = CrearMedico(1, "MP-100");
        var consultorioOriginal = CrearConsultorio(1, "101");
        var consultorioNuevo = CrearConsultorio(2, "102");
        var horario = CrearHorario(10, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico, consultorioOriginal);

        var command = new AsignarConsultorioAHorarioCommand(10, 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.IdConsultorio);
        Assert.Equal("102", result.Value.NumeroConsultorio);
        Assert.Equal(2, horario.IdConsultorio);
        Assert.Same(consultorioNuevo, horario.Consultorio);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_HorarioInexistente_RetornaFalloNotFound()
    {
        // Arrange
        CrearConsultorio(1, "101");
        var command = new AsignarConsultorioAHorarioCommand(999, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el horario", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ConsultorioInexistente_RetornaFalloNotFound()
    {
        // Arrange
        var medico = CrearMedico(1, "MP-100");
        var consultorio = CrearConsultorio(1, "101");
        CrearHorario(10, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico, consultorio);

        var command = new AsignarConsultorioAHorarioCommand(10, 999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el consultorio", result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ConsultorioOcupadoPorOtroMedicoEnMismaFranja_LanzaHorarioAtencionSuperpuestoException()
    {
        // Arrange: Medico 1 ya ocupa el consultorio 2 el Lunes de 08:00 a 12:00
        var medico1 = CrearMedico(1, "MP-100");
        var consultorio2 = CrearConsultorio(2, "102");
        CrearHorario(1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico1, consultorio2);

        // Medico 2 está en consultorio 1 el Lunes de 10:00 a 14:00 e intenta cambiarse al consultorio 2
        var medico2 = CrearMedico(2, "MP-200");
        var consultorio1 = CrearConsultorio(1, "101");
        var horarioMedico2 = CrearHorario(2, DayOfWeek.Monday, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0), medico2, consultorio1);

        var command = new AsignarConsultorioAHorarioCommand(2, 2);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HorarioAtencionSuperpuestoException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("102", ex.Message);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, _unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ConsultorioOcupadoEnDistintoHorario_AsignaSinProblemas()
    {
        // Arrange: Medico 1 ocupa consultorio 2 el Lunes de 08:00 a 12:00
        var medico1 = CrearMedico(1, "MP-100");
        var consultorio2 = CrearConsultorio(2, "102");
        CrearHorario(1, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), medico1, consultorio2);

        // Medico 2 está en consultorio 1 de 12:00 a 16:00 (contiguo, sin solapamiento) y se cambia a consultorio 2
        var medico2 = CrearMedico(2, "MP-200");
        var consultorio1 = CrearConsultorio(1, "101");
        var horarioMedico2 = CrearHorario(2, DayOfWeek.Monday, new TimeSpan(12, 0, 0), new TimeSpan(16, 0, 0), medico2, consultorio1);

        var command = new AsignarConsultorioAHorarioCommand(2, 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.IdConsultorio);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ObtenerConsultorios_RetornaListadoOrdenado()
    {
        // Arrange
        CrearConsultorio(3, "201", piso: 2, sector: "Ala Sur");
        CrearConsultorio(1, "101", piso: 1, sector: "Ala Norte");
        CrearConsultorio(2, "102", piso: 1, sector: "Ala Norte");

        // Act
        var result = await _obtenerConsultoriosHandler.Handle(new ObtenerConsultoriosQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal("101", result.Value[0].Numero);
        Assert.Equal("102", result.Value[1].Numero);
        Assert.Equal("201", result.Value[2].Numero);
        Assert.Contains("Consultorio N° 101", result.Value[0].Ubicacion);
    }
}
