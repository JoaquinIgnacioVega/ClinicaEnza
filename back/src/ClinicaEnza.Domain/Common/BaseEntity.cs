namespace ClinicaEnza.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public DateTime FechaCreacion { get; protected set; } = DateTime.UtcNow;
    public bool Activo { get; protected set; } = true;

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }
}

