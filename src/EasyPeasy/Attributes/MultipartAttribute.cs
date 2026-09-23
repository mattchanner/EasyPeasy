namespace EasyPeasy.Attributes;

/// <summary>
/// Sends the method's <see cref="FormParamAttribute"/> parameters as <c>multipart/form-data</c>.
/// Methods with a <see cref="FileInfo"/> or <see cref="FilePart"/> parameter are multipart automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class MultipartAttribute : Attribute
{
}
