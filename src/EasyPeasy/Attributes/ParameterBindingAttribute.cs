namespace EasyPeasy.Attributes;

/// <summary>Base class for attributes that bind a method parameter to part of the request.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public abstract class ParameterBindingAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="ParameterBindingAttribute"/> class.</summary>
    /// <param name="name">The name to bind to, or <see langword="null"/> to use the parameter name.</param>
    protected ParameterBindingAttribute(string? name) => Name = name;

    /// <summary>Gets the bound name, or <see langword="null"/> when the parameter name is used.</summary>
    public string? Name { get; }
}
