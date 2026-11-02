using System.Linq.Expressions;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Pacientes.Queries.ObtenerHistoriaClinicaPorPaciente;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerHistoriaClinicaPorPacienteQueryHandlerTests
{
    [Fact]
    public async Task Handle_PacienteConPrescripciones_RetornaHistoriaCompleta()
    {
        var paciente = new Paciente(
            "Ana", "Ríos", "11223344", new DateTime(1990, 1, 1), "ana@test.com", "OSDE", 70, 1.65m)
        {
            IdPersona = 1
        };
        paciente.HistoriaClinica!.IdHistoriaClinica = 10;

        var medico = new Medico(
            "Carlos", "Pérez", "22334455", new DateTime(1980, 1, 1), "carlos@test.com", Especialidad.Clinica, "MP-100")
        {
            IdPersona = 2
        };
        var prescripcion = paciente.HistoriaClinica.AgregarPrescripcion(2, "Aspirina 100mg", medico);
        prescripcion.IdPrescripcion = 20;

        var handler = new ObtenerHistoriaClinicaPorPacienteQueryHandler(
            new FakePacienteRepository(paciente));

        var result = await handler.Handle(new ObtenerHistoriaClinicaPorPacienteQuery(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("RÍOS, Ana", result.Value.NombrePaciente);
        Assert.Equal(70, result.Value.Peso);
        Assert.Equal(1.65m, result.Value.Altura);
        Assert.Contains("Aspirina 100mg", result.Value.MedicamentosRecetados);
        var prescripcionDto = Assert.Single(result.Value.Prescripciones);
        Assert.Equal("PÉREZ, Carlos", prescripcionDto.NombreMedico);
        Assert.Equal("MP-100", prescripcionDto.MatriculaMedico);
    }

    [Fact]
    public async Task Handle_PacienteInexistente_RetornaFallo()
    {
        var handler = new ObtenerHistoriaClinicaPorPacienteQueryHandler(
            new FakePacienteRepository());

        var result = await handler.Handle(new ObtenerHistoriaClinicaPorPacienteQuery(999));

        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el paciente", result.Error);
    }

    private sealed class FakePacienteRepository : IPacienteRepository
    {
        private readonly Paciente? _paciente;

        public FakePacienteRepository(Paciente? paciente = null)
        {
            _paciente = paciente;
        }

        public Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default) =>
            Task.FromResult(_paciente?.IdPersona == idPaciente ? _paciente : null);

        public Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
            Task.FromResult<Paciente?>(null);

        public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_paciente?.IdPersona == id ? _paciente : null);

        public Task<IReadOnlyList<Paciente>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Paciente>>(_paciente == null ? new List<Paciente>() : new List<Paciente> { _paciente });

        public Task<IReadOnlyList<Paciente>> FindAsync(Expression<Func<Paciente, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var pacientes = _paciente != null && predicate.Compile()(_paciente)
                ? new List<Paciente> { _paciente }
                : new List<Paciente>();
            return Task.FromResult<IReadOnlyList<Paciente>>(pacientes);
        }

        public Task AddAsync(Paciente entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Update(Paciente entity) { }

        public void Delete(Paciente entity) { }
    }
}
