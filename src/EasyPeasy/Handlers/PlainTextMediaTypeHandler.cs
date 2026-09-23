using System.Globalization;
using System.Text;

namespace EasyPeasy.Handlers;

/// <summary>
/// Reads and writes text. Strings are sent and received as they are; enums and other primitive types are
/// converted using the invariant culture, so a <c>text/plain</c> body of <c>42</c> can be read as an <see cref="int"/>.
/// </summary>
public sealed class PlainTextMediaTypeHandler : IMediaTypeHandler
{
    /// <inheritdoc />
    public HttpContent Serialize(object? value, Type type, string mediaType)
    {
        string text = value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        return new StringContent(text, Encoding.UTF8, mediaType);
    }

    /// <inheritdoc />
    public async ValueTask<object?> DeserializeAsync(HttpContent content, Type type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(type);

        string text = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (type == typeof(string) || type == typeof(object))
        {
            return text;
        }

        Type target = Nullable.GetUnderlyingType(type) ?? type;
        if (text.Length == 0 && target != type)
        {
            return null;
        }

        text = text.Trim();
        if (target.IsEnum)
        {
            return Enum.Parse(target, text, ignoreCase: true);
        }

        if (target == typeof(Guid))
        {
            return Guid.Parse(text);
        }

        if (target == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (target == typeof(DateTime))
        {
            return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (typeof(IConvertible).IsAssignableFrom(target))
        {
            return Convert.ChangeType(text, target, CultureInfo.InvariantCulture);
        }

        throw new EasyPeasyException($"'{MediaType.TextPlain}' content cannot be converted to '{type}'.");
    }
}
