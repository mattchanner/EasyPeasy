namespace EasyPeasy.Attributes;

/// <summary>
/// Defines the path of a service (when applied to an interface) or of a single method.
/// Method paths are appended to the interface path, which is appended to the client's base address.
/// Path variables are written as <c>{name}</c> and bound with <see cref="PathParamAttribute"/>.
/// </summary>
/// <param name="path">The path.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
public sealed class PathAttribute(string path) : Attribute
{
    /// <summary>Gets the path.</summary>
    public string Path { get; } = path;
}
