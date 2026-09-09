namespace GeneradoNominaSystem.Domain.Exceptions;

public class DominioException : Exception
{
    public DominioException(string mensaje)
        : base(mensaje)
    {
    }

    public DominioException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }
}
