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
        public string Nombre { get; protected set; } = string.Empty;
        public string Apellido { get; protected set; } = string.Empty;
        public string Dni { get; protected set; } = string.Empty;
        public DateTime FechaNacimiento { get; protected set; }
        public string Mail { get; protected set; } = string.Empty;

        protected Persona() { }

        protected Persona(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido no puede estar vacío.", nameof(apellido));
            if (string.IsNullOrWhiteSpace(dni)) throw new ArgumentException("El DNI no puede estar vacío.", nameof(dni));
            if (string.IsNullOrWhiteSpace(mail)) throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(mail));
            if (fechaNacimiento == default) throw new ArgumentException("La fecha de nacimiento no es válida.", nameof(fechaNacimiento));

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
