using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain
{
    public class Paciente : Persona
    {
        public string ObraSocial { get; private set; } = string.Empty;
        public decimal Peso { get; private set; }
        public decimal Altura { get; private set; }
        public string? PasswordHash { get; private set; }
        public bool DebeCambiarPassword { get; private set; } = true;

        // Propiedades de navegación
        public HistoriaClinica? HistoriaClinica { get; private set; }
        public List<Turno> Turnos { get; private set; } = new();

        protected Paciente() { }

        public Paciente(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, string obraSocial)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(obraSocial))
                throw new ArgumentException("La obra social no puede estar vacía.", nameof(obraSocial));

            ObraSocial = obraSocial;
            InicializarHistoriaClinica();
        }

        public Paciente(string nombre, string apellido, string dni, DateTime fechaNacimiento, string mail, string obraSocial, decimal peso, decimal altura)
            : base(nombre, apellido, dni, fechaNacimiento, mail)
        {
            if (string.IsNullOrWhiteSpace(obraSocial))
                throw new ArgumentException("La obra social no puede estar vacía.", nameof(obraSocial));

            ActualizarMedidas(peso, altura);
            ObraSocial = obraSocial;
            InicializarHistoriaClinica();
        }

        public void InicializarHistoriaClinica()
        {
            if (HistoriaClinica == null)
            {
                HistoriaClinica = new HistoriaClinica(this);
            }
        }

        public void ActualizarDatosPersonales(string nombre, string apellido, string mail, string obraSocial)
        {
            ActualizarDatos(nombre, apellido, mail);

            if (string.IsNullOrWhiteSpace(obraSocial))
                throw new ArgumentException("La obra social no puede estar vacía.", nameof(obraSocial));

            ObraSocial = obraSocial.Trim();
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

        public void ConfigurarPasswordInicial(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(passwordHash));

            PasswordHash = passwordHash;
            DebeCambiarPassword = true;
        }

        public void CambiarPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(passwordHash));

            PasswordHash = passwordHash;
            DebeCambiarPassword = false;
        }

        public override string ObtenerIdentificacion()
        {
            return $"Paciente DNI: {Dni} | Obra Social: {ObraSocial}";
        }
    }
}

