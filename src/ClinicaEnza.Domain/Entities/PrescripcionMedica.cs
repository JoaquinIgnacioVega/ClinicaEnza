using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class PrescripcionMedica
{
    public int IdPrescripcion { get; set; }
    public string Medicamento { get; set; } = string.Empty;

    // Claves foráneas
    public int IdHistoriaClinica { get; set; }
    public HistoriaClinica HistoriaClinica { get; set; } = null!;

    public int IdMedico { get; set; }
    public Medico Medico { get; set; } = null!;
}
