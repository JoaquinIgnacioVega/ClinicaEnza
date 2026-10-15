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
        public decimal Peso { get; private set; }
        public decimal Altura { get; private set; }

        // Propiedades de navegación
        public HistoriaClinica? HistoriaClinica { get; private set; }
        public List<Turno> Turnos { get; private set; } = new();

        protected Paciente() { }

        public Paciente(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, string obraSocial)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            ObraSocial = obraSocial;
        }

        public void ActualizarMedidas(decimal peso, decimal altura)
        {
            if (peso <= 0 || altura <= 0)
            {
                throw new ArgumentException("El peso y la altura deben ser valores mayores a cero.");
            }

            Peso = peso;
            Altura = altura;
        }

        public override string ObtenerIdentificacion()
        {
            return $"Paciente DNI: {Dni} | Obra Social: {ObraSocial}";
        }
    }
}
