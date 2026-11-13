using ClinicaEnza.Application.Features.Consultorios.Commands.DesactivarConsultorio;
using ClinicaEnza.Application.Features.Consultorios.Commands.ModificarConsultorio;
using ClinicaEnza.Domain;
using ClinicaEnza.Tests.Fakes;
using Xunit;

namespace ClinicaEnza.Tests;

public class ModificarConsultorioCommandHandlerTests
{
    [Fact]
    public async Task Modificar_DatosValidos_ActualizaConsultorio()
    {
        var repository = new FakeRepository<Consultorio>();
        var consultorio = new Consultorio("101", 1, "Norte") { IdConsultorio = 1 };
        repository.Entities.Add(consultorio);
        var result = await new ModificarConsultorioCommandHandler(repository, new FakeUnitOfWork()).Handle(new ModificarConsultorioCommand { IdConsultorio = 1, Numero = "202", Piso = 2, Sector = "Sur" });
        Assert.True(result.IsSuccess); Assert.Equal("202", consultorio.Numero); Assert.Equal(2, consultorio.Piso);
    }

    [Fact]
    public async Task Modificar_NumeroDuplicado_NoActualiza()
    {
        var repository = new FakeRepository<Consultorio>();
        repository.Entities.Add(new Consultorio("101", 1, "Norte") { IdConsultorio = 1 });
        repository.Entities.Add(new Consultorio("202", 2, "Sur") { IdConsultorio = 2 });
        var result = await new ModificarConsultorioCommandHandler(repository, new FakeUnitOfWork()).Handle(new ModificarConsultorioCommand { IdConsultorio = 1, Numero = "202", Piso = 2, Sector = "Sur" });
        Assert.True(result.IsFailure); Assert.Contains("existe", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Desactivar_ConsultorioExistente_LoMarcaInactivo()
    {
        var repository = new FakeRepository<Consultorio>();
        var consultorio = new Consultorio("101", 1, "Norte") { IdConsultorio = 1 }; repository.Entities.Add(consultorio);
        var result = await new DesactivarConsultorioCommandHandler(repository, new FakeUnitOfWork()).Handle(new DesactivarConsultorioCommand(1));
        Assert.True(result.IsSuccess); Assert.False(consultorio.Activo);
    }
}
