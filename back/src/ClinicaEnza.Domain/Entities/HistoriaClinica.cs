using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class HistoriaClinica
{
    public int IdHistoriaClinica { get; set; }

    // Relación 1:1 con Paciente
    public int IdPaciente { get; set; }
    public Paciente Paciente { get; private set; } = null!;

    // Relación 1:N con Prescripciones
    public List<PrescripcionMedica> Prescripciones { get; private set; } = new();

    protected HistoriaClinica() { }

    public HistoriaClinica(Paciente paciente)
    {
        Paciente = paciente ?? throw new ArgumentNullException(nameof(paciente));
        IdPaciente = paciente.IdPersona;
    }

    public void AgregarPrescripcion(PrescripcionMedica prescripcion)
    {
        if (prescripcion == null)
        {
            throw new ArgumentNullException(nameof(prescripcion), "La prescripción no puede ser nula.");
        }

        Prescripciones.Add(prescripcion);
    }

    public List<PrescripcionMedica> ObtenerPrescripcionesPorMedico(int idMedico)
    {
        return Prescripciones
            .Where(p => p.IdMedico == idMedico)
            .ToList();
    }

    public List<string> ObtenerMedicamentosRecetados()
    {
        return Prescripciones
            .Select(p => p.Medicamento)
            .Distinct()
            .ToList();
    }

    public PrescripcionMedica? BuscarPrimeraPrescripcionDe(string medicamento)
    {
        return Prescripciones
            .FirstOrDefault(p => p.Medicamento.Equals(medicamento, StringComparison.OrdinalIgnoreCase));
    }
}
