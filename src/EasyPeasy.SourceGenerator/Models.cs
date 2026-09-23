using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EasyPeasy.SourceGenerator;

internal sealed record InterfaceModel(
    string FullName,
    string DisplayName,
    EquatableArray<MethodModel> Methods,
    EquatableArray<DiagnosticInfo> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(d => d.Descriptor.DefaultSeverity == DiagnosticSeverity.Error);
}

internal enum ReturnKind
{
    Void,
    Result,
    ApiResponse,
    ApiResponseOf,
    AsyncEnumerable,
}

internal sealed record MethodModel(
    string DeclaringInterface,
    string Name,
    string ReturnType,
    ReturnKind ReturnKind,
    bool IsValueTask,
    string? ResultType,
    string HttpMethod,
    string Path,
    string? Accept,
    string? ContentType,
    EquatableArray<HeaderModel> Headers,
    bool IsMultipart,
    EquatableArray<ParameterModel> Parameters);

internal sealed record HeaderModel(string Name, string Value);

internal enum ParameterKind
{
    Path,
    Query,
    Header,
    Form,
    Body,
    CancellationToken,
}

internal sealed record ParameterModel(string Identifier, string Type, ParameterKind Kind, string Key);

/// <summary>A diagnostic stored without Roslyn symbols or syntax, so models stay cacheable.</summary>
internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, EquatableArray<string> Arguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol? symbol, params string[] arguments) =>
        new(descriptor, LocationInfo.From(symbol), new EquatableArray<string>(arguments));

    public Diagnostic ToDiagnostic() =>
        Diagnostic.Create(Descriptor, Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None, Arguments.ToArray<object>());
}

internal sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    public static LocationInfo? From(ISymbol? symbol)
    {
        var location = symbol?.Locations.FirstOrDefault(l => l.IsInSource);
        return location?.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);
}
