using ClinicaEnza.Application.Features.Consultorios.Commands.CrearConsultorio;
using ClinicaEnza.Domain;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class CrearConsultorioCommandHandlerTests
{
    [Fact]
    public async Task Handle_DatosValidos_CreaConsultorioYConfirmaTransaccion()
    {
        var repository = new FakeRepository<Consultorio>();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CrearConsultorioCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CrearConsultorioCommand { Numero = " 101 ", Piso = 1, Sector = " Ala Norte " });

        Assert.True(result.IsSuccess);
        Assert.Equal("101", result.Value.Numero);
        Assert.Equal("Ala Norte", result.Value.Sector);
        Assert.Single(repository.Entities);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, unitOfWork.CommitCallCount);
    }

    [Theory]
    [InlineData("", 1, "Ala Norte", "número")]
    [InlineData("101", -1, "Ala Norte", "piso")]
    [InlineData("101", 1, "", "sector")]
    public async Task Handle_DatosInvalidos_NoPersiste(string numero, int piso, string sector, string textoEsperado)
    {
        var repository = new FakeRepository<Consultorio>();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CrearConsultorioCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CrearConsultorioCommand { Numero = numero, Piso = piso, Sector = sector });

        Assert.True(result.IsFailure);
        Assert.Contains(textoEsperado, result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.Entities);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_NumeroDuplicado_NoPersiste()
    {
        var repository = new FakeRepository<Consultorio>();
        repository.Entities.Add(new Consultorio("101", 1, "Ala Norte"));
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CrearConsultorioCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CrearConsultorioCommand { Numero = "101", Piso = 2, Sector = "Ala Sur" });

        Assert.True(result.IsFailure);
        Assert.Contains("Ya existe", result.Error);
        Assert.Equal(1, repository.Entities.Count);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
