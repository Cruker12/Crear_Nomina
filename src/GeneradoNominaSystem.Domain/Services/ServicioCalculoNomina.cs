using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Services;

public sealed record ItemCalculoNomina(ConceptoNomina Concepto, Dinero Valor);

public sealed record ResultadoCalculoNomina(
    Dinero SubtotalDevengos,
    Dinero SubtotalDeducciones,
    Dinero TotalNeto,
    string Moneda);

public sealed class ServicioCalculoNomina
{
    public ResultadoCalculoNomina Calcular(
        string moneda,
        IEnumerable<ItemCalculoNomina> items,
        Dinero? baseCalculo = null)
    {
        if (string.IsNullOrWhiteSpace(moneda) || moneda.Trim().Length != 3)
        {
            throw new ReglaNegocioException("La moneda debe ser un código ISO de 3 letras.");
        }

        if (items is null)
        {
            throw new ReglaNegocioException("Los conceptos a calcular son obligatorios.");
        }

        var monedaNormalizada = moneda.Trim().ToUpperInvariant();
        var devengos = Dinero.Cero(monedaNormalizada);
        var deducciones = Dinero.Cero(monedaNormalizada);

        var ordenados = items
            .OrderBy(i => i.Concepto.Orden)
            .ToList();

        foreach (var item in ordenados)
        {
            if (item.Concepto is null)
            {
                throw new ReglaNegocioException("El concepto es obligatorio.");
            }

            if (!item.Concepto.Activo)
            {
                throw new ReglaNegocioException($"El concepto {item.Concepto.Nombre} está inactivo.");
            }

            var valor = ResolverValor(item, monedaNormalizada, baseCalculo);

            switch (item.Concepto.Tipo)
            {
                case TipoConcepto.Devengo:
                case TipoConcepto.Comision:
                    devengos = devengos.Sumar(valor);
                    break;
                case TipoConcepto.Deduccion:
                    deducciones = deducciones.Sumar(valor);
                    break;
                case TipoConcepto.Beneficio:
                    break;
                default:
                    throw new ReglaNegocioException($"Tipo de concepto no soportado: {item.Concepto.Tipo}.");
            }
        }

        var neto = devengos.Restar(deducciones);
        return new ResultadoCalculoNomina(devengos, deducciones, neto, monedaNormalizada);
    }

    private static Dinero ResolverValor(ItemCalculoNomina item, string moneda, Dinero? baseCalculo)
    {
        if (item.Concepto.EsPorcentaje)
        {
            if (item.Concepto.PorcentajeBase is null)
            {
                throw new ReglaNegocioException($"El concepto {item.Concepto.Nombre} es porcentaje pero no tiene base definida.");
            }

            if (baseCalculo is null)
            {
                throw new ReglaNegocioException($"El concepto {item.Concepto.Nombre} requiere base de cálculo.");
            }

            if (!string.Equals(baseCalculo.Moneda, moneda, StringComparison.OrdinalIgnoreCase))
            {
                throw new ReglaNegocioException($"La base de cálculo usa moneda {baseCalculo.Moneda}, se esperaba {moneda}.");
            }

            if (baseCalculo.Monto < 0m)
            {
                throw new ReglaNegocioException("La base de cálculo no puede ser negativa.");
            }

            return baseCalculo.MultiplicarPor(item.Concepto.PorcentajeBase.Value / 100m);
        }

        if (item.Valor is null)
        {
            throw new ReglaNegocioException($"El valor del concepto {item.Concepto.Nombre} es obligatorio.");
        }

        if (!string.Equals(item.Valor.Moneda, moneda, StringComparison.OrdinalIgnoreCase))
        {
            throw new ReglaNegocioException($"El concepto {item.Concepto.Nombre} usa moneda {item.Valor.Moneda}, se esperaba {moneda}.");
        }

        if (item.Valor.Monto < 0m)
        {
            throw new ReglaNegocioException($"El valor de {item.Concepto.Nombre} no puede ser negativo.");
        }

        return item.Valor;
    }
}
