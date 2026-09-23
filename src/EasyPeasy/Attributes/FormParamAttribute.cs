namespace EasyPeasy.Attributes;

/// <summary>
/// Binds a parameter to a form field. The request is sent as <c>application/x-www-form-urlencoded</c>,
/// or as <c>multipart/form-data</c> when the method has <see cref="MultipartAttribute"/> or uploads a file.
/// </summary>
public sealed class FormParamAttribute : ParameterBindingAttribute
{
    /// <summary>Binds to the form field with the same name as the parameter.</summary>
    public FormParamAttribute() : base(null) { }

    /// <summary>Binds to the named form field.</summary>
    /// <param name="name">The form field name.</param>
    public FormParamAttribute(string name) : base(name) { }

    /// <summary>Gets the form field name (kept for compatibility with EasyPeasy 2.x).</summary>
    public string? ParameterName => Name;
}
