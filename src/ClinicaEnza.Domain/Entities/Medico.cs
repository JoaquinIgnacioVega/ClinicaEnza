using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Medico : Persona
    {
        public string Especialidad { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Matricula { get; set; } = string.Empty;

        // Propiedades de navegación
        public List<Turno> Turnos { get; set; } = new();
        public List<PrescripcionMedica> Prescripciones { get; set; } = new();
    }
}
