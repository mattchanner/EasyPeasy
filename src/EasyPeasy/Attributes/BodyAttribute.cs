namespace EasyPeasy.Attributes;

/// <summary>
/// Marks a parameter as the request body. This is optional: a parameter with no binding attribute is the body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class BodyAttribute : Attribute
{
}
