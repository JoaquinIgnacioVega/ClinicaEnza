using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Usuario : Persona
    {
        public string Legajo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Rol { get; set; } = string.Empty;
    }
}
