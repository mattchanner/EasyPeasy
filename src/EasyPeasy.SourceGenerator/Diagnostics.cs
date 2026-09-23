using Microsoft.CodeAnalysis;

namespace EasyPeasy.SourceGenerator;

internal static class Diagnostics
{
    private const string Category = "EasyPeasy";

    public static readonly DiagnosticDescriptor MissingHttpMethod = Error(
        "EP0001",
        "Method has no HTTP method attribute",
        "Method '{0}' on '{1}' needs an HTTP method attribute such as [GET] or [POST]");

    public static readonly DiagnosticDescriptor UnsupportedMember = Error(
        "EP0002",
        "Unsupported interface member",
        "'{0}' on '{1}' is not supported: EasyPeasy interfaces can only declare methods");

    public static readonly DiagnosticDescriptor GenericMethod = Error(
        "EP0003",
        "Generic methods are not supported",
        "Method '{0}' is generic, which EasyPeasy does not support");

    public static readonly DiagnosticDescriptor GenericInterface = Error(
        "EP0004",
        "Generic interfaces are not supported",
        "Interface '{0}' is generic, which EasyPeasy does not support");

    public static readonly DiagnosticDescriptor UnsupportedReturnType = Error(
        "EP0005",
        "Unsupported return type",
        "Method '{0}' returns '{1}'. EasyPeasy methods must return Task, Task<T>, ValueTask, ValueTask<T> or IAsyncEnumerable<T>.");

    public static readonly DiagnosticDescriptor RefParameter = Error(
        "EP0006",
        "ref, out and in parameters are not supported",
        "Parameter '{0}' of '{1}' cannot be ref, out or in");

    public static readonly DiagnosticDescriptor MultipleBodies = Error(
        "EP0007",
        "More than one body parameter",
        "Method '{0}' has more than one body parameter. Parameters without a binding attribute are the body; add [QueryParam], [PathParam], [HeaderParam] or [FormParam] to the others.");

    public static readonly DiagnosticDescriptor BodyWithForm = Error(
        "EP0008",
        "Body and form parameters cannot be combined",
        "Method '{0}' has both a body parameter and form fields; a request has only one body");

    public static readonly DiagnosticDescriptor UnboundPathVariable = Error(
        "EP0009",
        "Path variable has no parameter",
        "Path '{0}' of method '{1}' contains '{{{2}}}', but no parameter has [PathParam(\"{2}\")]");

    public static readonly DiagnosticDescriptor UnknownPathParameter = Error(
        "EP0010",
        "Path parameter is not in the path",
        "Parameter '{0}' of '{1}' is bound to path variable '{2}', which is not in path '{3}'");

    public static readonly DiagnosticDescriptor MultipleBindings = Error(
        "EP0011",
        "Parameter has more than one binding attribute",
        "Parameter '{0}' of '{1}' has more than one of [PathParam], [QueryParam], [HeaderParam], [FormParam] and [Body]");

    public static readonly DiagnosticDescriptor InaccessibleInterface = Error(
        "EP0012",
        "Interface is not accessible",
        "Interface '{0}' must be public or internal (including any containing types) for EasyPeasy to implement it");

    public static readonly DiagnosticDescriptor MultipleHttpMethods = Error(
        "EP0013",
        "Method has more than one HTTP method attribute",
        "Method '{0}' has more than one HTTP method attribute");

    public static readonly DiagnosticDescriptor MultipleCancellationTokens = Error(
        "EP0014",
        "More than one CancellationToken parameter",
        "Method '{0}' has more than one CancellationToken parameter");

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
