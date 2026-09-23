namespace EasyPeasy.Attributes;

/// <summary>
/// Base class for the attributes that bind an interface method to an HTTP method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public abstract class HttpMethodAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="HttpMethodAttribute"/> class.</summary>
    /// <param name="path">Optional path relative to the interface <see cref="PathAttribute"/>.</param>
    protected HttpMethodAttribute(string? path) => Path = path;

    /// <summary>Gets the path relative to the interface <see cref="PathAttribute"/>, if one was supplied.</summary>
    public string? Path { get; }
}

/// <summary>Sends the request using the HTTP GET method.</summary>
public sealed class GETAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="GETAttribute"/> class.</summary>
    public GETAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="GETAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public GETAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP POST method.</summary>
public sealed class POSTAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="POSTAttribute"/> class.</summary>
    public POSTAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="POSTAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public POSTAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP PUT method.</summary>
public sealed class PUTAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="PUTAttribute"/> class.</summary>
    public PUTAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="PUTAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public PUTAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP DELETE method.</summary>
public sealed class DELETEAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="DELETEAttribute"/> class.</summary>
    public DELETEAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="DELETEAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public DELETEAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP PATCH method.</summary>
public sealed class PATCHAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="PATCHAttribute"/> class.</summary>
    public PATCHAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="PATCHAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public PATCHAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP HEAD method.</summary>
public sealed class HEADAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="HEADAttribute"/> class.</summary>
    public HEADAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="HEADAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public HEADAttribute(string path) : base(path) { }
}

/// <summary>Sends the request using the HTTP OPTIONS method.</summary>
public sealed class OPTIONSAttribute : HttpMethodAttribute
{
    /// <summary>Initializes a new instance of the <see cref="OPTIONSAttribute"/> class.</summary>
    public OPTIONSAttribute() : base(null) { }

    /// <summary>Initializes a new instance of the <see cref="OPTIONSAttribute"/> class.</summary>
    /// <param name="path">The path relative to the interface <see cref="PathAttribute"/>.</param>
    public OPTIONSAttribute(string path) : base(path) { }
}
