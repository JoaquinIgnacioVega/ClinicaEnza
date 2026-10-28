using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicaEnza.Domain.Enums;

namespace ClinicaEnza.Domain
{
    public class Usuario : Persona
    {
        public string Legajo { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime FechaInicio { get; private set; }
        public RolUsuario Rol { get; private set; }
        public int? IdMedico { get; private set; }
        public Medico? Medico { get; private set; }

        protected Usuario() { }

        public Usuario(
            string nombre,
            string apellido,
            string dni,
            DateTime fechaNacimiento,
            string mail,
            string legajo,
            string passwordHash,
            RolUsuario rol,
            int? idMedico = null,
            Medico? medico = null)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(legajo))
                throw new ArgumentException("El legajo es obligatorio.", nameof(legajo));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(passwordHash));

            if (rol == RolUsuario.Medico && !idMedico.HasValue && medico == null)
                throw new ArgumentException("Un usuario con rol Médico debe estar asociado a un médico.", nameof(idMedico));

            Legajo = legajo;
            PasswordHash = passwordHash;
            Rol = rol;
            FechaInicio = DateTime.Now;

            if (medico != null)
            {
                Medico = medico;
                IdMedico = medico.IdPersona;
            }
            else
            {
                IdMedico = idMedico;
            }
        }

        public Usuario(
            string nombre,
            string apellido,
            string dni,
            DateTime fechaNacimiento,
            string mail,
            string legajo,
            RolUsuario rol,
            Medico? medico = null)
            : this(nombre, apellido, dni, fechaNacimiento, mail, legajo, "DEFAULT_HASH", rol, medico?.IdPersona, medico)
        {
        }

        public void ActualizarPasswordHash(string nuevoPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(nuevoPasswordHash));

            PasswordHash = nuevoPasswordHash;
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

