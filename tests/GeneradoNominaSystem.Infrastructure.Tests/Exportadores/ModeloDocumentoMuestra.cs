using FluentAssertions;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Infrastructure.Tests.Exportadores;

public static class ModeloDocumentoMuestra
{
    public static ModeloDocumentoNomina CrearNomina()
    {
        return new ModeloDocumentoNomina(
            "Empresa Test S.A.S.",
            "900123456",
            "Calle 1 # 2-3, Medellín",
            "6041234567",
            "Juan Pérez",
            "12345678",
            "Marzo 2026",
            "NOM-2026-0001",
            new DateTime(2026, 3, 31),
            "Calculada",
            new List<LineaDocumentoNomina>
            {
                new("Salario", TipoConcepto.Devengo, null, 2000000m, "COP", 1),
                new("Salud", TipoConcepto.Deduccion, null, 80000m, "COP", 2),
                new("Pensión", TipoConcepto.Deduccion, null, 80000m, "COP", 3),
            },
            2000000m,
            160000m,
            1840000m,
            "COP");
    }

    public static ModeloDocumentoCotizacion CrearCotizacion()
    {
        return new ModeloDocumentoCotizacion(
            "Empresa Test S.A.S.",
            "900123456",
            "Calle 1 # 2-3, Medellín",
            "6041234567",
            "Cliente Test",
            "800111222",
            "COT-2026-0001",
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 31),
            "Enviada",
            new List<LineaDocumentoCotizacion>
            {
                new("Consultoría", 10m, 150000m, null, 1500000m, 1),
                new("Licencia", 1m, 900000m, 10m, 810000m, 2),
            },
            2310000m,
            2310000m,
            "COP",
            "Validez 15 días.");
    }
}
