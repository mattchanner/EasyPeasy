using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.Serialization;

namespace EasyPeasy.Xml;

/// <summary>
/// Reads and writes XML with <see cref="DataContractSerializer"/>, as EasyPeasy 2.x did by default.
/// </summary>
/// <remarks>
/// <see cref="DataContractSerializer"/> is synchronous, so responses are read into memory asynchronously
/// first rather than blocking on the network.
/// </remarks>
public sealed class XmlMediaTypeHandler : IMediaTypeHandler
{
    internal const string TrimmingMessage =
        "DataContractSerializer uses reflection over the serialized types, which trimming and Native AOT cannot analyze.";

    private const string SuppressionJustification =
        "The constructor carries the RequiresUnreferencedCode/RequiresDynamicCode warning for callers.";

    /// <summary>Initializes a new instance of the <see cref="XmlMediaTypeHandler"/> class.</summary>
    [RequiresUnreferencedCode(TrimmingMessage)]
    [RequiresDynamicCode(TrimmingMessage)]
    public XmlMediaTypeHandler()
    {
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    public HttpContent Serialize(object? value, Type type, string mediaType)
    {
        ArgumentNullException.ThrowIfNull(type);

        using var buffer = new MemoryStream();
        new DataContractSerializer(type).WriteObject(buffer, value);

        var content = new ByteArrayContent(buffer.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType) { CharSet = "utf-8" };
        return content;
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = SuppressionJustification)]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = SuppressionJustification)]
    public async ValueTask<object?> DeserializeAsync(HttpContent content, Type type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(type);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;
        return new DataContractSerializer(type).ReadObject(buffer);
    }
}
