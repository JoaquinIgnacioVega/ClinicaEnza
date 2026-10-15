using ClinicaEnza.Domain;

namespace ClinicaEnza.Application.Common.Interfaces.Persistence;

public interface IPacienteRepository : IRepository<Paciente>
{
    Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerConHistoriaClinicaAsync(int idPaciente, CancellationToken cancellationToken = default);
}

