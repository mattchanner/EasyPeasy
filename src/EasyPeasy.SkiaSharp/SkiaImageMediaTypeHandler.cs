using System.Net.Http.Headers;
using SkiaSharp;

namespace EasyPeasy.SkiaSharp;

/// <summary>
/// Reads responses as <see cref="SKBitmap"/>, <see cref="SKImage"/> or <see cref="SKData"/>, and encodes those
/// types for request bodies. (Methods can also use <c>byte[]</c> or <see cref="Stream"/> for images with no add-on.)
/// </summary>
/// <param name="format">The format used to encode request bodies.</param>
/// <param name="quality">The encoding quality, from 0 to 100.</param>
public sealed class SkiaImageMediaTypeHandler(SKEncodedImageFormat format, int quality = 100) : IMediaTypeHandler
{
    /// <summary>Gets the format used to encode request bodies.</summary>
    public SKEncodedImageFormat Format { get; } = format;

    /// <summary>Gets the encoding quality.</summary>
    public int Quality { get; } = quality;

    /// <inheritdoc />
    public HttpContent Serialize(object? value, Type type, string mediaType)
    {
        SKData? data = value switch
        {
            SKBitmap bitmap => bitmap.Encode(Format, Quality),
            SKImage image => image.Encode(Format, Quality),
            SKData raw => raw,
            _ => throw new ArgumentException(
                $"Cannot send '{value?.GetType().Name ?? "null"}' as {mediaType}. Expected SKBitmap, SKImage or SKData.", nameof(value)),
        };

        if (data is null)
        {
            throw new NotSupportedException($"SkiaSharp cannot encode images as {Format}.");
        }

        using (data)
        {
            var content = new ByteArrayContent(data.ToArray());
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return content;
        }
    }

    /// <inheritdoc />
    public async ValueTask<object?> DeserializeAsync(HttpContent content, Type type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(type);

        byte[] bytes = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if (type == typeof(SKImage))
        {
            return SKImage.FromEncodedData(bytes);
        }

        if (type == typeof(SKData))
        {
            return SKData.CreateCopy(bytes);
        }

        if (type == typeof(SKBitmap) || type == typeof(object))
        {
            return SKBitmap.Decode(bytes);
        }

        throw new EasyPeasyException($"Image content cannot be read as '{type}'. Use SKBitmap, SKImage, SKData or byte[].");
    }
}
