namespace EasyPeasy.Attributes;

/// <summary>
/// The media type the client expects to receive. It is sent as the <c>Accept</c> header, and picks the
/// handler used when a response has no <c>Content-Type</c>.
/// </summary>
/// <param name="mediaType">The media type.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
public sealed class ConsumesAttribute(string mediaType) : Attribute
{
    /// <summary>Gets the media type.</summary>
    public string MediaType { get; } = mediaType;
}
