namespace EasyPeasy;

/// <summary>Configures how an EasyPeasy client builds requests and reads responses.</summary>
public sealed class EasyPeasySettings
{
    /// <summary>Gets or sets the handlers used to serialize request bodies and deserialize responses.</summary>
    public MediaTypeHandlerRegistry MediaTypeHandlers { get; set; } = MediaTypeHandlerRegistry.CreateDefault();

    /// <summary>Gets or sets the formatter for path, query, header and form values.</summary>
    public IUrlParameterFormatter UrlParameterFormatter { get; set; } = DefaultUrlParameterFormatter.Instance;

    /// <summary>
    /// Gets or sets the media type used when neither <see cref="Attributes.ConsumesAttribute"/> nor
    /// <see cref="Attributes.ProducesAttribute"/> is specified. Defaults to <c>application/json</c>.
    /// </summary>
    public string DefaultMediaType { get; set; } = MediaType.ApplicationJson;
}
