namespace EasyPeasy.Attributes;

/// <summary>
/// Binds a parameter to a <c>{name}</c> variable in the path. Values are always escaped, so they cannot
/// add path segments.
/// </summary>
public sealed class PathParamAttribute : ParameterBindingAttribute
{
    /// <summary>Binds to the path variable with the same name as the parameter.</summary>
    public PathParamAttribute() : base(null) { }

    /// <summary>Binds to the named path variable.</summary>
    /// <param name="name">The path variable name.</param>
    public PathParamAttribute(string name) : base(name) { }

    /// <summary>Gets the path variable name (kept for compatibility with EasyPeasy 2.x).</summary>
    public string? ParameterName => Name;
}
