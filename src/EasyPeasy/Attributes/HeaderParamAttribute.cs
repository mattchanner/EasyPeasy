namespace EasyPeasy.Attributes;

/// <summary>
/// Binds a parameter to a request header. <see langword="null"/> values are omitted. Content headers such
/// as <c>Content-Type</c> are applied to the request body.
/// </summary>
public sealed class HeaderParamAttribute : ParameterBindingAttribute
{
    /// <summary>Binds to the header with the same name as the parameter.</summary>
    public HeaderParamAttribute() : base(null) { }

    /// <summary>Binds to the named header.</summary>
    /// <param name="name">The header name.</param>
    public HeaderParamAttribute(string name) : base(name) { }

    /// <summary>Gets the header name (kept for compatibility with EasyPeasy 2.x).</summary>
    public string? HeaderName => Name;
}
