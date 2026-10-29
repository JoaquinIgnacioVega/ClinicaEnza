using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Medico : Persona
    {
        public Especialidad Especialidad { get; private set; }
        public DateTime FechaInicio { get; private set; }
        public string Matricula { get; private set; } = string.Empty;
        public List<Turno> Turnos { get; private set; } = new();
        public List<PrescripcionMedica> Prescripciones { get; private set; } = new();

        protected Medico() { }

        public Medico(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, Especialidad especialidad, string matricula, DateTime fechaInicio)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                throw new ArgumentException("La matrícula es obligatoria.", nameof(matricula));
            if (!Enum.IsDefined(typeof(Especialidad), especialidad))
                throw new ArgumentException("La especialidad no es válida.", nameof(especialidad));
            if (fechaInicio == default)
                throw new ArgumentException("La fecha de inicio es obligatoria.", nameof(fechaInicio));

            Especialidad = especialidad;
            Matricula = matricula;
            FechaInicio = fechaInicio;
        }

        public Medico(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, Especialidad especialidad, string matricula)
            : this(nombre, apellido, dni, fechaNacimiento, mail, especialidad, matricula, DateTime.Today)
        {
        }

        public void ActualizarInformacion(string nombre, string apellido, string mail, Especialidad especialidad, string matricula)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException("El apellido no puede estar vacío.", nameof(apellido));
            if (string.IsNullOrWhiteSpace(mail))
                throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(mail));
            if (string.IsNullOrWhiteSpace(matricula))
                throw new ArgumentException("La matrícula es obligatoria.", nameof(matricula));
            if (!Enum.IsDefined(typeof(Especialidad), especialidad))
                throw new ArgumentException("La especialidad no es válida.", nameof(especialidad));

            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Mail = mail.Trim();
            Especialidad = especialidad;
            Matricula = matricula.Trim();
        }

        public string ObtenerFichaProfesional()
        {
            return $"Dr/a. {ObtenerNombreCompleto()} - MP: {Matricula} ({Especialidad})";
        }

        public override string ObtenerIdentificacion()
        {
            return $"Médico Matrícula: {Matricula}";
        }
    }
}
