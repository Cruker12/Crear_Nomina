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
}
