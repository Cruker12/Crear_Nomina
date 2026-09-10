using GeneradoNominaSystem.Domain.Interfaces.Services;

namespace GeneradoNominaSystem.Infrastructure.Services;

public sealed class NotificadorDocumentos : INotificadorDocumentos
{
    public event EventHandler<DocumentoGeneradoArgs>? DocumentoGenerado;

    public void Notificar(Guid referenciaId, string rutaArchivo)
    {
        DocumentoGenerado?.Invoke(this, new DocumentoGeneradoArgs(referenciaId, rutaArchivo));
    }
}
