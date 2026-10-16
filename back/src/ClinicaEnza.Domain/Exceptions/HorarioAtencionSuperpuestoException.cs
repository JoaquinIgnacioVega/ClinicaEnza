namespace ClinicaEnza.Domain.Exceptions;

public class HorarioAtencionSuperpuestoException : ReglaDeNegocioException
{
    public HorarioAtencionSuperpuestoException(string mensaje) : base(mensaje)
    {
    }
}
