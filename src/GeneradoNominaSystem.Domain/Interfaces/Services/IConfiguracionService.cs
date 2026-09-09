namespace GeneradoNominaSystem.Domain.Interfaces.Services;

public interface IConfiguracionService
{
    string? Obtener(string clave);

    void Establecer(string clave, string? valor);
}
