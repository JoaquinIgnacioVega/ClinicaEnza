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

        protected Usuario() { }

        public Usuario(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, string legajo, string rol)
        : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(legajo)) throw new ArgumentException("El legajo es obligatorio.", nameof(legajo));
            if (string.IsNullOrWhiteSpace(rol)) throw new ArgumentException("El rol es obligatorio.", nameof(rol));

            Legajo = legajo;
            Rol = rol;
            FechaInicio = DateTime.Now;
        }

        public bool EsAdministrador()
        {
            return Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
        }

        public override string ObtenerIdentificacion()
        {
            return $"Usuario Legajo: {Legajo} ({Rol})";
        }
    }
}
