using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class Consultorio
{
    public int IdConsultorio { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Sector { get; set; } = string.Empty;

    // Propiedad de navegación
    public List<Turno> Turnos { get; set; } = new();
}
