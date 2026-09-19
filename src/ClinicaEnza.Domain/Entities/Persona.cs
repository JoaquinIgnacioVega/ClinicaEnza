using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public abstract class Persona
    {
        public int IdPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Mail { get; set; } = string.Empty;
    }
}
