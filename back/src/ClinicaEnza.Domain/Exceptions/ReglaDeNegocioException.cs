namespace ClinicaEnza.Domain.Exceptions;

public class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string mensaje) : base(mensaje)
    {
    }

    public ReglaDeNegocioException(string mensaje, Exception innerException) : base(mensaje, innerException)
    {
    }
}

