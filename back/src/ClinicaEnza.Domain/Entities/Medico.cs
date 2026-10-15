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

        public Medico(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, Especialidad especialidad, string matricula)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                throw new ArgumentException("La matrícula es obligatoria.", nameof(matricula));

            Especialidad = especialidad;
            Matricula = matricula;
            FechaInicio = DateTime.Now;
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
