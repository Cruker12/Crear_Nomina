namespace GeneradoNominaSystem.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime FechaCreacion { get; protected set; } = DateTime.UtcNow;

    public DateTime? FechaModificacion { get; protected set; }

    protected void MarcarModificacion()
    {
        FechaModificacion = DateTime.UtcNow;
    }
}
