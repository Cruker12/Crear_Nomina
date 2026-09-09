namespace GeneradoNominaSystem.Domain.Exceptions;

public sealed class ReglaNegocioException : DominioException
{
    public ReglaNegocioException(string mensaje)
        : base(mensaje)
    {
    }

    public ReglaNegocioException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }
}
