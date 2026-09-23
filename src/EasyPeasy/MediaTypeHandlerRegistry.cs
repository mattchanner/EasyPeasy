using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using EasyPeasy.Handlers;

namespace EasyPeasy;

/// <summary>
/// Maps media types to the <see cref="IMediaTypeHandler"/> that reads and writes them.
/// </summary>
/// <remarks>
/// Lookups try the exact media type first, then the structured syntax suffix
/// (<c>application/problem+json</c> uses the <c>application/json</c> handler, <c>+xml</c> uses <c>application/xml</c>),
/// then <c>text/plain</c> for any other <c>text/*</c> type.
/// </remarks>
public sealed class MediaTypeHandlerRegistry
{
    private readonly ConcurrentDictionary<string, IMediaTypeHandler> handlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a registry with JSON (<c>application/json</c>) and text (<c>text/plain</c>, <c>text/html</c>) handlers.
    /// </summary>
    /// <param name="jsonSerializerOptions">
    /// The JSON options to use. Defaults to <see cref="JsonSerializerDefaults.Web"/>. Pass options with a
    /// <c>JsonSerializerContext</c> resolver for trimmed or Native AOT applications.
    /// </param>
    /// <returns>The registry.</returns>
    public static MediaTypeHandlerRegistry CreateDefault(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var registry = new MediaTypeHandlerRegistry();
        var text = new PlainTextMediaTypeHandler();
        registry.Register(MediaType.ApplicationJson, new JsonMediaTypeHandler(jsonSerializerOptions));
        registry.Register(MediaType.TextPlain, text);
        registry.Register(MediaType.TextHtml, text);
        return registry;
    }

    /// <summary>Registers or replaces the handler for a media type.</summary>
    /// <param name="mediaType">The media type, for example <c>application/json</c>.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>This registry, for chaining.</returns>
    public MediaTypeHandlerRegistry Register(string mediaType, IMediaTypeHandler handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);
        ArgumentNullException.ThrowIfNull(handler);
        handlers[mediaType] = handler;
        return this;
    }

    /// <summary>Removes the handler for a media type.</summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns><see langword="true"/> if a handler was removed.</returns>
    public bool Remove(string mediaType) => handlers.TryRemove(mediaType, out _);

    /// <summary>Finds the handler for a media type.</summary>
    /// <param name="mediaType">The media type, without parameters such as <c>charset</c>.</param>
    /// <param name="handler">The handler, if one was found.</param>
    /// <returns><see langword="true"/> if a handler was found.</returns>
    public bool TryGetHandler(string mediaType, [NotNullWhen(true)] out IMediaTypeHandler? handler)
    {
        ArgumentNullException.ThrowIfNull(mediaType);

        if (handlers.TryGetValue(mediaType, out handler))
        {
            return true;
        }

        if (mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
        {
            return handlers.TryGetValue(MediaType.ApplicationJson, out handler);
        }

        if (mediaType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase))
        {
            return handlers.TryGetValue(MediaType.ApplicationXml, out handler);
        }

        if (mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            return handlers.TryGetValue(MediaType.TextPlain, out handler);
        }

        return false;
    }
}
