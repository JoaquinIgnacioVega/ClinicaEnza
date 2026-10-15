using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface ITurnoRepository : IRepository<Turno>
{
    Task<IReadOnlyList<Turno>> ObtenerSuperpuestosAsync(int idMedico, DateTime inicio, DateTime fin, int? turnoIdExcluir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerSuperpuestosConsultorioAsync(int idConsultorio, DateTime inicio, DateTime fin, int? turnoIdExcluir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerPorMedicoAsync(int idMedico, CancellationToken cancellationToken = default);
    Task<Turno?> ObtenerDetallePorIdAsync(int idTurno, CancellationToken cancellationToken = default);
}

