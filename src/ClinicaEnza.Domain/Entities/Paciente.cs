using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Paciente : Persona
    {
        public string ObraSocial { get; set; } = string.Empty;
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }

        // Propiedades de navegación
        public HistoriaClinica? HistoriaClinica { get; set; }
        public List<Turno> Turnos { get; set; } = new();
    }
}
