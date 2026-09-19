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
    public Paciente Paciente { get; set; } = null!;

    // Relación 1:N con Prescripciones
    public List<PrescripcionMedica> Prescripciones { get; set; } = new();
}
