namespace GeneradoNominaSystem.Domain.ValueObjects;

public abstract class ValueObjectBase : IEquatable<ValueObjectBase>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObjectBase? other)
    {
        if (other is null || GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as ValueObjectBase);

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(17, (acc, h) => acc * 23 + h);
    }

    public static bool operator ==(ValueObjectBase? a, ValueObjectBase? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ValueObjectBase? a, ValueObjectBase? b) => !(a == b);
}
