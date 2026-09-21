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
        public string Nombre { get; private set; } = string.Empty;
        public string Apellido { get; private set; } = string.Empty;
        public string Dni { get; private set; } = string.Empty;
        public DateTime FechaNacimiento { get; private set; }
        public string Mail { get; private set; } = string.Empty;

        protected Persona() { }

        protected Persona(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido no puede estar vacío.", nameof(apellido));
            if (string.IsNullOrWhiteSpace(dni)) throw new ArgumentException("El DNI no puede estar vacío.", nameof(dni));

            Nombre = nombre;
            Apellido = apellido;
            Dni = dni;
            FechaNacimiento = fechaNacimiento;
            Mail = mail;
        }

        public string ObtenerNombreCompleto()
        {
            return $"{Apellido.ToUpper()}, {Nombre}";
        }

        public int CalcularEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;

            if (FechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }

        public abstract string ObtenerIdentificacion();
    }
}
