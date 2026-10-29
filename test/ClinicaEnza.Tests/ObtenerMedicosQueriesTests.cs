using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClinicaEnza.Application.Common.Interfaces.Persistence;
using ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicoPorId;
using ClinicaEnza.Application.Features.Medicos.Queries.ObtenerMedicos;
using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;
using Xunit;

namespace ClinicaEnza.Tests;

public class ObtenerMedicosQueriesTests
{
    private readonly FakeMedicoRepository _medicoRepository;
    private readonly ObtenerMedicosQueryHandler _obtenerMedicosHandler;
    private readonly ObtenerMedicoPorIdQueryHandler _obtenerMedicoPorIdHandler;

    public ObtenerMedicosQueriesTests()
    {
        _medicoRepository = new FakeMedicoRepository();
        _obtenerMedicosHandler = new ObtenerMedicosQueryHandler(_medicoRepository);
        _obtenerMedicoPorIdHandler = new ObtenerMedicoPorIdQueryHandler(_medicoRepository);
    }

    private void CargarMedicosDePrueba()
    {
        _medicoRepository.Medicos.AddRange(new[]
        {
            new Medico("Carlos", "Gomez", "20111222", new DateTime(1980, 5, 10), "carlos@test.com", Especialidad.Cardiologia, "MP-100", new DateTime(2015, 1, 1)) { IdPersona = 1 },
            new Medico("Laura", "Perez", "22333444", new DateTime(1985, 3, 15), "laura@test.com", Especialidad.Pediatria, "MP-200", new DateTime(2018, 3, 1)) { IdPersona = 2 },
            new Medico("Martin", "Lopez", "25666777", new DateTime(1978, 11, 20), "martin@test.com", Especialidad.Cardiologia, "MP-300", new DateTime(2010, 6, 1)) { IdPersona = 3 }
        });
    }

    [Fact]
    public async Task ObtenerMedicos_SinFiltro_RetornaTodosLosMedicos()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicosQuery();

        // Act
        var result = await _obtenerMedicosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count());
    }

    [Fact]
    public async Task ObtenerMedicos_ConFiltroEspecialidad_RetornaSoloMedicosDeEsaEspecialidad()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicosQuery(Especialidad.Cardiologia);

        // Act
        var result = await _obtenerMedicosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var medicos = result.Value.ToList();
        Assert.Equal(2, medicos.Count);
        Assert.All(medicos, m => Assert.Equal(Especialidad.Cardiologia, m.Especialidad));
    }

    [Fact]
    public async Task ObtenerMedicos_ConFiltroSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicosQuery(Especialidad.Dermatologia);

        // Act
        var result = await _obtenerMedicosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task ObtenerMedicos_MapeaFichaProfesionalCorrectamente()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicosQuery(Especialidad.Pediatria);

        // Act
        var result = await _obtenerMedicosHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var medico = result.Value.Single();
        Assert.Equal("Dr/a. PEREZ, Laura - MP: MP-200 (Pediatria)", medico.FichaProfesional);
    }

    [Fact]
    public async Task ObtenerMedicoPorId_Existente_RetornaMedicoDtoConFichaProfesional()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicoPorIdQuery(1);

        // Act
        var result = await _obtenerMedicoPorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.IdMedico);
        Assert.Equal("Carlos", result.Value.Nombre);
        Assert.Equal("Gomez", result.Value.Apellido);
        Assert.Equal(Especialidad.Cardiologia, result.Value.Especialidad);
        Assert.Equal("Dr/a. GOMEZ, Carlos - MP: MP-100 (Cardiologia)", result.Value.FichaProfesional);
    }

    [Fact]
    public async Task ObtenerMedicoPorId_Inexistente_RetornaFalloNotFound()
    {
        // Arrange
        CargarMedicosDePrueba();
        var query = new ObtenerMedicoPorIdQuery(999);

        // Act
        var result = await _obtenerMedicoPorIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("No se encontró el médico con ID 999", result.Error);
    }

    #region Fakes

    private class FakeMedicoRepository : IRepository<Medico>
    {
        public List<Medico> Medicos { get; } = new();

        public Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Medicos.FirstOrDefault(m => m.IdPersona == id));

        public Task<IReadOnlyList<Medico>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.ToList());

        public Task<IReadOnlyList<Medico>> FindAsync(Expression<Func<Medico, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Medico>>(Medicos.AsQueryable().Where(predicate).ToList());

        public Task AddAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            Medicos.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Medico entity) { }
        public void Delete(Medico entity) => Medicos.Remove(entity);
    }

    #endregion
}
