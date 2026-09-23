namespace EasyPeasy.Attributes;

/// <summary>
/// Binds a parameter to a query string parameter. <see langword="null"/> values are omitted and
/// sequences are sent as repeated parameters.
/// </summary>
public sealed class QueryParamAttribute : ParameterBindingAttribute
{
    /// <summary>Binds to the query parameter with the same name as the parameter.</summary>
    public QueryParamAttribute() : base(null) { }

    /// <summary>Binds to the named query parameter.</summary>
    /// <param name="name">The query parameter name.</param>
    public QueryParamAttribute(string name) : base(name) { }

    /// <summary>Gets the query parameter name (kept for compatibility with EasyPeasy 2.x).</summary>
    public string? ParameterName => Name;
}
