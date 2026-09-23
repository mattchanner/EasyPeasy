namespace EasyPeasy.Attributes;

/// <summary>
/// Adds a fixed header to every request made through the interface or method it is applied to.
/// Method headers are added after interface headers, and <see cref="HeaderParamAttribute"/> values replace both.
/// </summary>
/// <param name="name">The header name.</param>
/// <param name="value">The header value.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class HeaderAttribute(string name, string value) : Attribute
{
    /// <summary>Gets the header name.</summary>
    public string Name { get; } = name;

    /// <summary>Gets the header value.</summary>
    public string Value { get; } = value;
}
