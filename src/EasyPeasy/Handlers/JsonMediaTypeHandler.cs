using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EasyPeasy.Handlers;

/// <summary>
/// Reads and writes JSON with System.Text.Json. The request body is serialized as it is sent, and
/// responses are read from the stream, so neither is buffered in memory first.
/// </summary>
/// <remarks>
/// Types are resolved through <see cref="JsonSerializerOptions.TypeInfoResolver"/>. For trimmed or Native AOT
/// applications, supply options whose resolver is a <c>JsonSerializerContext</c> that includes every type sent or received.
/// </remarks>
public sealed class JsonMediaTypeHandler : IStreamingMediaTypeHandler
{
    /// <summary>Initializes a new instance of the <see cref="JsonMediaTypeHandler"/> class.</summary>
    /// <param name="options">The serializer options. Defaults to <see cref="JsonSerializerDefaults.Web"/>.</param>
    public JsonMediaTypeHandler(JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // GetTypeInfo needs a resolver. Fall back to reflection only where the app allows it: under Native AOT
        // the switch is off, so a missing JsonSerializerContext surfaces as a clear NotSupportedException.
        if (options.TypeInfoResolver is null && JsonSerializer.IsReflectionEnabledByDefault)
        {
            options = WithReflectionResolver(options);
        }

        Options = options;
    }

    /// <summary>Gets the serializer options.</summary>
    public JsonSerializerOptions Options { get; }

    /// <inheritdoc />
    public HttpContent Serialize(object? value, Type type, string mediaType)
    {
        var contentType = new MediaTypeHeaderValue(mediaType) { CharSet = "utf-8" };
        return JsonContent.Create(value, Options.GetTypeInfo(type), contentType);
    }

    /// <inheritdoc />
    public async ValueTask<object?> DeserializeAsync(HttpContent content, Type type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer.DeserializeAsync(stream, Options.GetTypeInfo(type), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<T?> DeserializeAsyncEnumerable<T>(
        HttpContent content,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        var typeInfo = (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));
        var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(stream, typeInfo, cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = ReflectionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = ReflectionJustification)]
    private static JsonSerializerOptions WithReflectionResolver(JsonSerializerOptions options) =>
        new(options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    private const string ReflectionJustification =
        "Only called when JsonSerializer.IsReflectionEnabledByDefault is true. Trimmed and Native AOT publishing " +
        "turn that switch off by default, and the trimmer then removes the call.";
}
