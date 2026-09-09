using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.ValueObjects;

public sealed class Dinero : ValueObjectBase
{
    public const string MonedaPorDefecto = "COP";

    public decimal Monto { get; }

    public string Moneda { get; }

    public Dinero(decimal monto, string moneda = MonedaPorDefecto)
    {
        if (string.IsNullOrWhiteSpace(moneda) || moneda.Trim().Length != 3)
        {
            throw new ReglaNegocioException("La moneda debe ser un código ISO de 3 letras.");
        }

        Monto = Math.Round(monto, 2, MidpointRounding.ToEven);
        Moneda = moneda.Trim().ToUpperInvariant();
    }

    public bool EsCero => Monto == 0m;

    public bool EsPositivo => Monto > 0m;

    public Dinero Sumar(Dinero otro)
    {
        ValidarMismaMoneda(otro);
        return new Dinero(Monto + otro.Monto, Moneda);
    }

    public Dinero Restar(Dinero otro)
    {
        ValidarMismaMoneda(otro);
        return new Dinero(Monto - otro.Monto, Moneda);
    }

    public Dinero MultiplicarPor(decimal factor)
    {
        return new Dinero(Monto * factor, Moneda);
    }

    public string Formatear(string? simbolo = null, int decimales = 2)
    {
        var prefijo = simbolo ?? "$";
        var formato = $"N{decimales}";
        return $"{prefijo}{Monto.ToString(formato)} {Moneda}";
    }

    private void ValidarMismaMoneda(Dinero otro)
    {
        if (otro is null)
        {
            throw new ReglaNegocioException("No se puede operar con un valor nulo.");
        }

        if (!string.Equals(Moneda, otro.Moneda, StringComparison.OrdinalIgnoreCase))
        {
            throw new ReglaNegocioException($"No se puede operar {Moneda} con {otro.Moneda}.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monto;
        yield return Moneda;
    }

    public static Dinero Cero(string moneda = MonedaPorDefecto) => new(0m, moneda);

    public static Dinero operator +(Dinero a, Dinero b) => a.Sumar(b);

    public static Dinero operator -(Dinero a, Dinero b) => a.Restar(b);

    public static Dinero operator *(Dinero a, decimal factor) => a.MultiplicarPor(factor);

    public override string ToString() => Formatear();
}
