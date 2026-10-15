using System;

namespace ClinicaEnza.Domain;

public class HorarioAtencion
{
    public int IdHorarioAtencion { get; set; }
    public DayOfWeek DiaSemana { get; private set; }
    public TimeSpan HoraDesde { get; private set; }
    public TimeSpan HoraHasta { get; private set; }

    // Claves foráneas y objetos de navegación
    public int IdMedico { get; private set; }
    public Medico Medico { get; private set; } = null!;

    public int IdConsultorio { get; private set; }
    public Consultorio Consultorio { get; private set; } = null!;

    protected HorarioAtencion() { }

    public HorarioAtencion(DayOfWeek diaSemana, TimeSpan horaDesde, TimeSpan horaHasta, Medico medico, Consultorio consultorio)
    {
        if (horaHasta <= horaDesde)
            throw new ArgumentException("La hora hasta debe ser posterior a la hora desde.", nameof(horaHasta));

        DiaSemana = diaSemana;
        HoraDesde = horaDesde;
        HoraHasta = horaHasta;

        Medico = medico ?? throw new ArgumentNullException(nameof(medico));
        Consultorio = consultorio ?? throw new ArgumentNullException(nameof(consultorio));

        IdMedico = medico.IdPersona;
        IdConsultorio = consultorio.IdConsultorio;
    }

    public bool IncluyeHorario(DateTime horario)
    {
        if (horario.DayOfWeek != DiaSemana)
            return false;

        var horaDelDia = horario.TimeOfDay;
        return horaDelDia >= HoraDesde && horaDelDia < HoraHasta;
    }

    public bool EsSolapadoCon(HorarioAtencion otroHorario)
    {
        if (otroHorario == null) return false;
        if (DiaSemana != otroHorario.DiaSemana) return false;

        return HoraDesde < otroHorario.HoraHasta && otroHorario.HoraDesde < HoraHasta;
    }
}