using System.Globalization;

namespace EasyPeasy;

/// <summary>Formats path, query, header and form values as strings.</summary>
public interface IUrlParameterFormatter
{
    /// <summary>Formats a value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The formatted value (unescaped), or <see langword="null"/> to omit it.</returns>
    string? Format(object? value);
}

/// <summary>
/// The default <see cref="IUrlParameterFormatter"/>. It is culture invariant: booleans are <c>true</c>/<c>false</c>,
/// dates use ISO 8601, and enums use their names.
/// </summary>
public sealed class DefaultUrlParameterFormatter : IUrlParameterFormatter
{
    /// <summary>Gets the shared instance.</summary>
    public static DefaultUrlParameterFormatter Instance { get; } = new();

    /// <inheritdoc />
    public string? Format(object? value) => value switch
    {
        null => null,
        string s => s,
        bool b => b ? "true" : "false",
        DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly t => t.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture),
        Enum e => e.ToString(),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}
