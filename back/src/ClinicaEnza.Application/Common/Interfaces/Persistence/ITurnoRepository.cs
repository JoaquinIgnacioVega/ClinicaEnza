using ClinicaEnza.Domain;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface ITurnoRepository : IRepository<Turno>
{
    Task<IReadOnlyList<Turno>> ObtenerSuperpuestosAsync(int idMedico, DateTime inicio, DateTime fin, int? turnoIdExcluir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerSuperpuestosConsultorioAsync(int idConsultorio, DateTime inicio, DateTime fin, int? turnoIdExcluir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerSuperpuestosPacienteAsync(int idPaciente, DateTime inicio, DateTime fin, int? turnoIdExcluir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerPorMedicoAsync(int idMedico, DateTime? fecha = null, EstadoTurno? estado = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerAgendaMedicoAsync(int idMedico, DateTime? fecha = null, EstadoTurno? estado = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> ObtenerPorPacienteAsync(int idPaciente, bool soloPendientes = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turno>> BuscarTurnosAsync(DateTime? fecha, int? idPaciente, int? idMedico, EstadoTurno? estado, CancellationToken cancellationToken = default);
    Task<Turno?> ObtenerDetallePorIdAsync(int idTurno, CancellationToken cancellationToken = default);
}

