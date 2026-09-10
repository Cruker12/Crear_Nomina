namespace GeneradoNominaSystem.Domain.Interfaces.Services;

/// <summary>
/// Datos del aviso de documento generado.
/// </summary>
public sealed class DocumentoGeneradoArgs : EventArgs
{
    public DocumentoGeneradoArgs(Guid referenciaId, string rutaArchivo)
    {
        ReferenciaId = referenciaId;
        RutaArchivo = rutaArchivo;
    }

    public Guid ReferenciaId { get; }

    public string RutaArchivo { get; }
}

/// <summary>
/// Puerto para avisar que se generó un documento (PDF/Excel).
/// El Historial lo escucha para mantenerse actualizado.
/// Debe registrarse como singleton para que emisor y oyentes compartan instancia.
/// </summary>
public interface INotificadorDocumentos
{
    event EventHandler<DocumentoGeneradoArgs>? DocumentoGenerado;

    void Notificar(Guid referenciaId, string rutaArchivo);
}
