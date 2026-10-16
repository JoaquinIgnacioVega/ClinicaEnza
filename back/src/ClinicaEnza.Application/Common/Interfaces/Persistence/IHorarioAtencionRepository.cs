using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IHorarioAtencionRepository
{
    Task<IReadOnlyList<HorarioAtencion>> ObtenerPorMedicoAsync(
        int idMedico,
        CancellationToken cancellationToken = default);
}
