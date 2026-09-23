namespace EasyPeasy.Attributes;

/// <summary>
/// The media type the client sends in the request body. It selects the serializer and sets the
/// <c>Content-Type</c> header.
/// </summary>
/// <param name="mediaType">The media type.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
public sealed class ProducesAttribute(string mediaType) : Attribute
{
    /// <summary>Gets the media type.</summary>
    public string MediaType { get; } = mediaType;
}
