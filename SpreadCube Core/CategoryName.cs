namespace SpreadCube_Core;

/// <summary>
/// Essentially a string which disallows invalid category names.
/// </summary>
public class CategoryName
{
    public string Value { get; private set; }

    public CategoryName(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value) + " is null.");
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(nameof(value) + " is not a valid category name.");

        Value = value.Trim();
    }

    public override bool Equals(object? obj) =>
        obj is CategoryName name && Value == name.Value;

    public override int GetHashCode() =>
        HashCode.Combine(Value);
}