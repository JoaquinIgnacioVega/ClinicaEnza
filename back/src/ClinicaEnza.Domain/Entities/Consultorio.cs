using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class Consultorio
{
    public int IdConsultorio { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Piso { get; set; }
    public string Sector { get; set; } = string.Empty;

    // Propiedad de navegación
    public List<Turno> Turnos { get; private set; } = new();

    protected Consultorio() { }

    public Consultorio(string numero, int piso, string sector)
    {
        if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("El número de consultorio es obligatorio.", nameof(numero));

        Numero = numero;
        Piso = piso;
        Sector = sector;
    }

    public string ObtenerUbicacion()
    {
        return $"Consultorio N° {Numero} (Piso {Piso}, Sector {Sector})";
    }

    public bool EstaDisponibleEn(DateTime horario)
    {
        // Un consultorio no está disponible si ya existe un turno activo en ese horario exacto
        return !Turnos.Any(t => t.HoraInicio == horario && t.Estado != EstadoTurno.Cancelado);
    }
}
