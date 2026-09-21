using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaEnza.Domain;

public class Turno
{
    public int IdTurno { get; set; }
    public DateTime Horario { get; private set; }
    public string Estado { get; private set; } = "Pendiente";
    // Claves foráneas
    public int IdPaciente { get; set; }
    public Paciente Paciente { get; private set; } = null!;

    public int IdMedico { get; set; }
    public Medico Medico { get; private set; } = null!;

    public int IdConsultorio { get; set; }
    public Consultorio Consultorio { get; private set; } = null!;

    protected Turno() { }

    public Turno(DateTime horario, Paciente paciente, Medico medico, Consultorio consultorio)
    {
        if (horario <= DateTime.Now)
            throw new ArgumentException("El horario del turno debe ser una fecha futura.", nameof(horario));

        Horario = horario;
        Paciente = paciente ?? throw new ArgumentNullException(nameof(paciente));
        Medico = medico ?? throw new ArgumentNullException(nameof(medico));
        Consultorio = consultorio ?? throw new ArgumentNullException(nameof(consultorio));

        IdPaciente = paciente.IdPersona;
        IdMedico = medico.IdPersona;
        IdConsultorio = consultorio.IdConsultorio;
        Estado = "Pendiente";
    }

    public void Reagendar(DateTime nuevoHorario)
    {
        if (nuevoHorario <= DateTime.Now)
        {
            throw new InvalidOperationException("No se puede reagendar un turno a una fecha u hora pasada.");
        }

        Horario = nuevoHorario;
        Estado = "Reagendado";
    }

    public void CambiarEstado(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado))
        {
            throw new ArgumentException("El nuevo estado no puede estar vacío.", nameof(nuevoEstado));
        }

        Estado = nuevoEstado;
    }

    public bool EsSolapadoCon(DateTime horarioComparar)
    {
        // Se asume un bloque estándar de consulta de 30 minutos
        var finTurnoActual = Horario.AddMinutes(30);
        var finTurnoComparar = horarioComparar.AddMinutes(30);

        return Horario < finTurnoComparar && horarioComparar < finTurnoActual;
    }
}
