namespace ClinicaEnza.Domain.Exceptions;

public class TurnoSuperpuestoException : ReglaDeNegocioException
{
    public int IdMedico { get; }
    public DateTime FechaHora { get; }

    public TurnoSuperpuestoException(int idMedico, DateTime fechaHora)
        : base($"El médico con ID {idMedico} ya tiene un turno asignado en el horario {fechaHora:yyyy-MM-dd HH:mm} o existe superposición.")
    {
        IdMedico = idMedico;
        FechaHora = fechaHora;
    }

    public TurnoSuperpuestoException(string mensaje) : base(mensaje)
    {
    }
}

