using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class Turno
{
    public int IdTurno { get; set; }
    public DateTime Horario { get; set; }

    // Claves foráneas
    public int IdPaciente { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int IdMedico { get; set; }
    public Medico Medico { get; set; } = null!;

    public int IdConsultorio { get; set; }
    public Consultorio Consultorio { get; set; } = null!;
}
