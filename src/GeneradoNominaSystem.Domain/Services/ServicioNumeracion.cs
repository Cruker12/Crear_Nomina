using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces.Services;

namespace GeneradoNominaSystem.Domain.Services;

public sealed class ServicioNumeracion : IServicioNumeracion
{
    public string GenerarNumero(string prefijo, int anio, int secuencia)
    {
        if (string.IsNullOrWhiteSpace(prefijo))
        {
            throw new ReglaNegocioException("El prefijo es obligatorio.");
        }

        if (anio < 2000 || anio > 2100)
        {
            throw new ReglaNegocioException("El año no es válido.");
        }

        if (secuencia < 1)
        {
            throw new ReglaNegocioException("La secuencia debe ser mayor o igual a 1.");
        }

        return $"{prefijo.Trim().ToUpperInvariant()}-{anio}-{secuencia:0000}";
    }
}
