using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicaEnza.Domain.Exceptions;

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

    public void Reagendar(DateTime nuevaHoraInicio, Consultorio? nuevoConsultorio = null)
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new ReglaDeNegocioException("Solo se puede reagendar un turno pendiente.");

        if (nuevaHoraInicio <= DateTime.Now)
            throw new ReglaDeNegocioException("No se puede reagendar a una fecha pasada.");

        HoraInicio = nuevaHoraInicio;
        CalcularHoraFin();

        if (nuevoConsultorio != null)
        {
            Consultorio = nuevoConsultorio;
            IdConsultorio = nuevoConsultorio.IdConsultorio;
        }

        Estado = EstadoTurno.Pendiente;
    }

    public void CambiarEstado(EstadoTurno nuevoEstado)
    {
        Estado = nuevoEstado;
    }

    public void Cancelar()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new ReglaDeNegocioException("Solo se puede cancelar un turno pendiente.");

        Estado = EstadoTurno.Cancelado;
    }

    public void CancelarPorPaciente(int idPacienteSolicitante, DateTime fechaActual)
    {
        if (IdPaciente != idPacienteSolicitante)
            throw new ReglaDeNegocioException("No tienes permiso para cancelar un turno que pertenece a otro paciente.");

        if (Estado != EstadoTurno.Pendiente)
            throw new ReglaDeNegocioException("Solo se pueden cancelar turnos que se encuentren en estado pendiente.");

        if (HoraInicio <= fechaActual)
            throw new ReglaDeNegocioException("No se puede cancelar un turno cuya fecha y hora ya han vencido.");

        Estado = EstadoTurno.Cancelado;
    }

    public void MarcarComoRealizado()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new ReglaDeNegocioException("Solo se puede marcar como realizado un turno pendiente.");

        Estado = EstadoTurno.Realizado;
    }

    public bool EsSolapadoCon(Turno otroTurno)
    {
        if (otroTurno == null || Estado == EstadoTurno.Cancelado || otroTurno.Estado == EstadoTurno.Cancelado)
            return false;

        return HoraInicio < otroTurno.HoraFin && otroTurno.HoraInicio < HoraFin;
    }
}

