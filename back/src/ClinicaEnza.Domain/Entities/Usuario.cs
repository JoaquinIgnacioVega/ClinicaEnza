using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Usuario : Persona
    {
        public string Legajo { get; private set; } = string.Empty;
        public DateTime FechaInicio { get; private set; }
        public RolUsuario Rol { get; private set; }
        public int? IdMedico { get; private set; }
        public Medico? Medico { get; private set; }

        protected Usuario() { }

        public Usuario(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, string legajo, RolUsuario rol, Medico? medico = null)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(legajo))
                throw new ArgumentException("El legajo es obligatorio.", nameof(legajo));

            Legajo = legajo;
            Rol = rol;
            FechaInicio = DateTime.Now;

            if (medico != null)
            {
                Medico = medico;
                IdMedico = medico.IdPersona;
            }
        }

        public bool EsAdministrador()
        {
            return Rol == RolUsuario.Administrador;
        }

        public override string ObtenerIdentificacion()
        {
            return $"Usuario Legajo: {Legajo} ({Rol})";
        }
    }
}
