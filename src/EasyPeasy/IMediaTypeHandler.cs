namespace EasyPeasy;

/// <summary>
/// Converts between .NET objects and HTTP content for one or more media types.
/// </summary>
public interface IMediaTypeHandler
{
    /// <summary>Creates the request content for <paramref name="value"/>.</summary>
    /// <param name="value">The value to send.</param>
    /// <param name="type">The declared type of the value.</param>
    /// <param name="mediaType">The media type to send, used as the <c>Content-Type</c>.</param>
    /// <returns>The content. It can serialize lazily when it is sent.</returns>
    HttpContent Serialize(object? value, Type type, string mediaType);

    /// <summary>Reads a value of <paramref name="type"/> from response content.</summary>
    /// <param name="content">The response content.</param>
    /// <param name="type">The type to read.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The value that was read.</returns>
    ValueTask<object?> DeserializeAsync(HttpContent content, Type type, CancellationToken cancellationToken);
}

/// <summary>
/// A <see cref="IMediaTypeHandler"/> that can also read a sequence of items as they arrive,
/// for methods that return <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
public interface IStreamingMediaTypeHandler : IMediaTypeHandler
{
    /// <summary>Reads items from response content as they arrive.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="content">The response content.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The items.</returns>
    IAsyncEnumerable<T?> DeserializeAsyncEnumerable<T>(HttpContent content, CancellationToken cancellationToken);
}
