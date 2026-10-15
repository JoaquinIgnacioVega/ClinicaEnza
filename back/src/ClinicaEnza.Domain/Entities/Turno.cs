using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class Turno
{
    public int IdTurno { get; set; }
    public DateTime HoraInicio { get; private set; }
    public DateTime HoraFin { get; private set; }
    public EstadoTurno Estado { get; private set; } = EstadoTurno.Pendiente;

    public int IdPaciente { get; private set; }
    public Paciente Paciente { get; private set; } = null!;

    public int IdMedico { get; private set; }
    public Medico Medico { get; private set; } = null!;

    public int IdConsultorio { get; private set; }
    public Consultorio Consultorio { get; private set; } = null!;

    protected Turno() { }

    public Turno(DateTime horaInicio, Paciente paciente, Medico medico, Consultorio consultorio)
    {
        if (horaInicio <= DateTime.Now)
            throw new ArgumentException("La hora de inicio debe ser una fecha futura.", nameof(horaInicio));

        HoraInicio = horaInicio;
        CalcularHoraFin();

        Paciente = paciente ?? throw new ArgumentNullException(nameof(paciente));
        Medico = medico ?? throw new ArgumentNullException(nameof(medico));
        Consultorio = consultorio ?? throw new ArgumentNullException(nameof(consultorio));

        IdPaciente = paciente.IdPersona;
        IdMedico = medico.IdPersona;
        IdConsultorio = consultorio.IdConsultorio;
    }

    private void CalcularHoraFin()
    {
        HoraFin = HoraInicio.AddMinutes(20); // Duración fija definida en diagramaSecuencia.md
    }

    public void Reagendar(DateTime nuevaHoraInicio)
    {
        if (nuevaHoraInicio <= DateTime.Now)
            throw new InvalidOperationException("No se puede reagendar a una fecha pasada.");

        HoraInicio = nuevaHoraInicio;
        CalcularHoraFin();
        Estado = EstadoTurno.Pendiente;
    }

    public void CambiarEstado(EstadoTurno nuevoEstado)
    {
        Estado = nuevoEstado;
    }

    public bool EsSolapadoCon(Turno otroTurno)
    {
        if (otroTurno == null) return false;
        return HoraInicio < otroTurno.HoraFin && otroTurno.HoraInicio < HoraFin;
    }
}
