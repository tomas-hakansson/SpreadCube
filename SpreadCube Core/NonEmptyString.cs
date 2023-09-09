namespace SpreadCube_Core;

/// <summary>
/// Essentially a string which disallows invalid category names.
/// </summary>
public class NonEmptyString
{
    public string Value { get; private set; }

    public NonEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(nameof(value) + " cannot be empty nor consist solely of whitespace.");

        Value = value.Trim();
    }

    public static explicit operator string(NonEmptyString nes) =>
        nes.Value;

    public static implicit operator NonEmptyString(string str) =>
        new(str);

    public static bool operator ==(NonEmptyString nes1, NonEmptyString nes2) =>
        nes1.Equals(nes2);

    public static bool operator !=(NonEmptyString nes1, NonEmptyString nes2) =>
        !(nes1 == nes2);

    public override bool Equals(object? obj) =>
        obj is NonEmptyString name && Value == name.Value;

    public override int GetHashCode() =>
        HashCode.Combine(Value);
}