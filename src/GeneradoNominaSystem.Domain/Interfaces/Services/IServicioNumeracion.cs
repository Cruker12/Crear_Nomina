namespace GeneradoNominaSystem.Domain.Interfaces.Services;

public interface IServicioNumeracion
{
    string GenerarNumero(string prefijo, int anio, int secuencia);
}
