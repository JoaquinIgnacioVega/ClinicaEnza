using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IHorarioAtencionRepository
{
    Task<HorarioAtencion?> ObtenerPorMedicoYHorarioAsync(
        int idMedico,
        DayOfWeek diaSemana,
        TimeSpan hora,
        TimeSpan horaFin,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HorarioAtencion>> ObtenerPorMedicoAsync(
        int idMedico,
        CancellationToken cancellationToken = default);
}
