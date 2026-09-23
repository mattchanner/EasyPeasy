using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasyPeasy.SourceGenerator;

/// <summary>Turns an interface symbol into an <see cref="InterfaceModel"/>, collecting diagnostics on the way.</summary>
internal sealed class Parser
{
    private const string AttributesNamespace = "EasyPeasy.Attributes.";

    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly Regex PathVariable = new(@"\{([^{}]+)\}", RegexOptions.Compiled);

    private readonly INamedTypeSymbol httpMethodAttribute;
    private readonly INamedTypeSymbol? pathAttribute;
    private readonly INamedTypeSymbol? consumesAttribute;
    private readonly INamedTypeSymbol? producesAttribute;
    private readonly INamedTypeSymbol? headerAttribute;
    private readonly INamedTypeSymbol? multipartAttribute;
    private readonly INamedTypeSymbol? pathParamAttribute;
    private readonly INamedTypeSymbol? queryParamAttribute;
    private readonly INamedTypeSymbol? formParamAttribute;
    private readonly INamedTypeSymbol? headerParamAttribute;
    private readonly INamedTypeSymbol? bodyAttribute;
    private readonly INamedTypeSymbol? task;
    private readonly INamedTypeSymbol? taskOfT;
    private readonly INamedTypeSymbol? valueTask;
    private readonly INamedTypeSymbol? valueTaskOfT;
    private readonly INamedTypeSymbol? asyncEnumerable;
    private readonly INamedTypeSymbol? cancellationToken;
    private readonly INamedTypeSymbol? apiResponse;
    private readonly INamedTypeSymbol? apiResponseOfT;
    private readonly INamedTypeSymbol? fileInfo;
    private readonly INamedTypeSymbol? filePart;
    private readonly INamedTypeSymbol? enumerableOfT;

    private readonly List<DiagnosticInfo> diagnostics = [];

    private Parser(Compilation compilation, INamedTypeSymbol httpMethodAttribute)
    {
        this.httpMethodAttribute = httpMethodAttribute;
        pathAttribute = Attribute(compilation, "PathAttribute");
        consumesAttribute = Attribute(compilation, "ConsumesAttribute");
        producesAttribute = Attribute(compilation, "ProducesAttribute");
        headerAttribute = Attribute(compilation, "HeaderAttribute");
        multipartAttribute = Attribute(compilation, "MultipartAttribute");
        pathParamAttribute = Attribute(compilation, "PathParamAttribute");
        queryParamAttribute = Attribute(compilation, "QueryParamAttribute");
        formParamAttribute = Attribute(compilation, "FormParamAttribute");
        headerParamAttribute = Attribute(compilation, "HeaderParamAttribute");
        bodyAttribute = Attribute(compilation, "BodyAttribute");
        task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        taskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        valueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        valueTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        asyncEnumerable = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
        cancellationToken = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        apiResponse = compilation.GetTypeByMetadataName("EasyPeasy.ApiResponse");
        apiResponseOfT = compilation.GetTypeByMetadataName("EasyPeasy.ApiResponse`1");
        fileInfo = compilation.GetTypeByMetadataName("System.IO.FileInfo");
        filePart = compilation.GetTypeByMetadataName("EasyPeasy.FilePart");
        enumerableOfT = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
    }

    /// <summary>
    /// Parses <paramref name="symbol"/> if it is an EasyPeasy interface: one where it, or an interface it
    /// inherits, declares a method with an HTTP method attribute. Returns <see langword="null"/> otherwise.
    /// </summary>
    public static InterfaceModel? TryParse(INamedTypeSymbol symbol, Compilation compilation, CancellationToken cancellationToken)
    {
        if (symbol.TypeKind != TypeKind.Interface)
        {
            return null;
        }

        var httpMethodAttribute = compilation.GetTypeByMetadataName(AttributesNamespace + "HttpMethodAttribute");
        if (httpMethodAttribute is null)
        {
            return null;
        }

        var parser = new Parser(compilation, httpMethodAttribute);
        var members = AllMembers(symbol).ToList();
        if (!members.OfType<IMethodSymbol>().Any(m => parser.HttpMethodAttributes(m).Any()))
        {
            return null;
        }

        return parser.Parse(symbol, members, cancellationToken);
    }

    private static INamedTypeSymbol? Attribute(Compilation compilation, string name) =>
        compilation.GetTypeByMetadataName(AttributesNamespace + name);

    private static IEnumerable<ISymbol> AllMembers(INamedTypeSymbol symbol) =>
        symbol.GetMembers().Concat(symbol.AllInterfaces.SelectMany(i => i.GetMembers()));

    private static string Display(ISymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

    private static string Identifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;

    private static string CombinePaths(string servicePath, string methodPath)
    {
        string service = servicePath.TrimEnd('/');
        if (methodPath.Length == 0)
        {
            return service;
        }

        return service + "/" + methodPath.TrimStart('/');
    }

    private static string? StringArgument(AttributeData? attribute, int index = 0) =>
        attribute is not null && attribute.ConstructorArguments.Length > index
            ? attribute.ConstructorArguments[index].Value as string
            : null;

    private static string HttpMethodExpression(INamedTypeSymbol attributeClass)
    {
        string verb = attributeClass.Name.EndsWith("Attribute", StringComparison.Ordinal)
            ? attributeClass.Name.Substring(0, attributeClass.Name.Length - "Attribute".Length)
            : attributeClass.Name;

        return verb.ToUpperInvariant() switch
        {
            "GET" => "global::System.Net.Http.HttpMethod.Get",
            "POST" => "global::System.Net.Http.HttpMethod.Post",
            "PUT" => "global::System.Net.Http.HttpMethod.Put",
            "DELETE" => "global::System.Net.Http.HttpMethod.Delete",
            "PATCH" => "global::System.Net.Http.HttpMethod.Patch",
            "HEAD" => "global::System.Net.Http.HttpMethod.Head",
            "OPTIONS" => "global::System.Net.Http.HttpMethod.Options",
            _ => $"new global::System.Net.Http.HttpMethod({SymbolDisplay.FormatLiteral(verb.ToUpperInvariant(), quote: true)})",
        };
    }

    private InterfaceModel Parse(INamedTypeSymbol symbol, List<ISymbol> members, CancellationToken cancellationToken)
    {
        string fullName = symbol.ToDisplayString(TypeFormat);

        if (symbol.IsGenericType)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.GenericInterface, symbol, Display(symbol)));
        }

        for (ISymbol? current = symbol; current is INamedTypeSymbol type; current = type.ContainingType)
        {
            if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
            {
                diagnostics.Add(DiagnosticInfo.Create(Diagnostics.InaccessibleInterface, symbol, Display(symbol)));
                break;
            }
        }

        var methods = new List<MethodModel>();
        foreach (var member in members)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Static members and default interface implementations need no generated code.
            if (member.IsStatic || !member.IsAbstract)
            {
                continue;
            }

            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method)
            {
                if (ParseMethod(method) is { } model)
                {
                    methods.Add(model);
                }
            }
            else if (member is IPropertySymbol or IEventSymbol)
            {
                diagnostics.Add(DiagnosticInfo.Create(Diagnostics.UnsupportedMember, member, member.Name, Display(member.ContainingType)));
            }
        }

        return new InterfaceModel(fullName, Display(symbol), methods.ToEquatableArray(), diagnostics.ToEquatableArray());
    }

    private IEnumerable<AttributeData> HttpMethodAttributes(IMethodSymbol method) =>
        method.GetAttributes().Where(a => InheritsFrom(a.AttributeClass, httpMethodAttribute));

    private static bool InheritsFrom(INamedTypeSymbol? type, INamedTypeSymbol? baseType)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static AttributeData? Find(ISymbol symbol, INamedTypeSymbol? attributeType) =>
        attributeType is null
            ? null
            : symbol.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));

    private MethodModel? ParseMethod(IMethodSymbol method)
    {
        var declaringInterface = method.ContainingType;
        string methodDisplay = Display(method);
        int errorsBefore = diagnostics.Count;

        var verbs = HttpMethodAttributes(method).ToList();
        if (verbs.Count == 0)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.MissingHttpMethod, method, method.Name, Display(declaringInterface)));
            return null;
        }

        if (verbs.Count > 1)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.MultipleHttpMethods, method, methodDisplay));
        }

        if (method.IsGenericMethod)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.GenericMethod, method, methodDisplay));
        }

        var verb = verbs[0];
        string methodPath = StringArgument(verb) ?? StringArgument(Find(method, pathAttribute)) ?? string.Empty;
        string servicePath = StringArgument(Find(declaringInterface, pathAttribute)) ?? string.Empty;
        string path = CombinePaths(servicePath, methodPath);

        string? accept = StringArgument(Find(method, consumesAttribute)) ?? StringArgument(Find(declaringInterface, consumesAttribute));
        string? contentType = StringArgument(Find(method, producesAttribute)) ?? StringArgument(Find(declaringInterface, producesAttribute));

        var headers = StaticHeaders(declaringInterface).Concat(StaticHeaders(method)).ToEquatableArray();
        bool isMultipart = Find(method, multipartAttribute) is not null;

        var (returnKind, isValueTask, resultType) = ClassifyReturnType(method);

        var parameters = new List<ParameterModel>();
        foreach (var parameter in method.Parameters)
        {
            if (ParseParameter(method, parameter) is { } model)
            {
                parameters.Add(model);
                isMultipart |= model.Kind == ParameterKind.Form && IsFile(parameter.Type);
            }
        }

        ValidateParameters(method, methodDisplay, path, parameters);

        if (diagnostics.Count > errorsBefore)
        {
            return null;
        }

        return new MethodModel(
            declaringInterface.ToDisplayString(TypeFormat),
            method.Name,
            method.ReturnType.ToDisplayString(TypeFormat),
            returnKind,
            isValueTask,
            resultType,
            HttpMethodExpression(verb.AttributeClass!),
            path,
            accept,
            contentType,
            headers,
            isMultipart,
            parameters.ToEquatableArray());
    }

    private IEnumerable<HeaderModel> StaticHeaders(ISymbol symbol) =>
        headerAttribute is null
            ? []
            : symbol.GetAttributes()
                .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, headerAttribute))
                .Select(a => new HeaderModel(StringArgument(a) ?? string.Empty, StringArgument(a, 1) ?? string.Empty));

    private (ReturnKind Kind, bool IsValueTask, string? ResultType) ClassifyReturnType(IMethodSymbol method)
    {
        var returnType = method.ReturnType as INamedTypeSymbol;
        var definition = returnType?.OriginalDefinition;

        if (SymbolEqualityComparer.Default.Equals(returnType, task))
        {
            return (ReturnKind.Void, false, null);
        }

        if (SymbolEqualityComparer.Default.Equals(returnType, valueTask))
        {
            return (ReturnKind.Void, true, null);
        }

        bool isTaskOfT = SymbolEqualityComparer.Default.Equals(definition, taskOfT);
        bool isValueTaskOfT = SymbolEqualityComparer.Default.Equals(definition, valueTaskOfT);
        if (isTaskOfT || isValueTaskOfT)
        {
            var result = returnType!.TypeArguments[0];
            if (SymbolEqualityComparer.Default.Equals(result, apiResponse))
            {
                return (ReturnKind.ApiResponse, isValueTaskOfT, null);
            }

            if (result is INamedTypeSymbol namedResult &&
                SymbolEqualityComparer.Default.Equals(namedResult.OriginalDefinition, apiResponseOfT))
            {
                return (ReturnKind.ApiResponseOf, isValueTaskOfT, namedResult.TypeArguments[0].ToDisplayString(TypeFormat));
            }

            return (ReturnKind.Result, isValueTaskOfT, result.ToDisplayString(TypeFormat));
        }

        if (SymbolEqualityComparer.Default.Equals(definition, asyncEnumerable))
        {
            return (ReturnKind.AsyncEnumerable, false, returnType!.TypeArguments[0].ToDisplayString(TypeFormat));
        }

        diagnostics.Add(DiagnosticInfo.Create(Diagnostics.UnsupportedReturnType, method, Display(method), method.ReturnType.ToDisplayString()));
        return (ReturnKind.Void, false, null);
    }

    private ParameterModel? ParseParameter(IMethodSymbol method, IParameterSymbol parameter)
    {
        if (parameter.RefKind != RefKind.None)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.RefParameter, parameter, parameter.Name, Display(method)));
            return null;
        }

        var bindings = new List<(AttributeData Attribute, ParameterKind Kind)>();
        foreach (var attribute in parameter.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            ParameterKind? kind =
                SymbolEqualityComparer.Default.Equals(attributeClass, pathParamAttribute) ? ParameterKind.Path :
                SymbolEqualityComparer.Default.Equals(attributeClass, queryParamAttribute) ? ParameterKind.Query :
                SymbolEqualityComparer.Default.Equals(attributeClass, formParamAttribute) ? ParameterKind.Form :
                SymbolEqualityComparer.Default.Equals(attributeClass, headerParamAttribute) ? ParameterKind.Header :
                SymbolEqualityComparer.Default.Equals(attributeClass, bodyAttribute) ? ParameterKind.Body :
                null;

            if (kind is not null)
            {
                bindings.Add((attribute, kind.Value));
            }
        }

        if (bindings.Count > 1)
        {
            // Carry on with the first binding so the error does not cascade into path variable errors.
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.MultipleBindings, parameter, parameter.Name, Display(method)));
        }

        string identifier = Identifier(parameter.Name);
        string type = parameter.Type.ToDisplayString(TypeFormat);

        if (bindings.Count > 0)
        {
            var (attribute, kind) = bindings[0];
            return new ParameterModel(identifier, type, kind, StringArgument(attribute) ?? parameter.Name);
        }

        if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationToken))
        {
            return new ParameterModel(identifier, type, ParameterKind.CancellationToken, parameter.Name);
        }

        return new ParameterModel(identifier, type, IsFile(parameter.Type) ? ParameterKind.Form : ParameterKind.Body, parameter.Name);
    }

    private bool IsFile(ITypeSymbol type)
    {
        bool IsFileType(ITypeSymbol t) =>
            SymbolEqualityComparer.Default.Equals(t, fileInfo) || SymbolEqualityComparer.Default.Equals(t, filePart);

        if (IsFileType(type))
        {
            return true;
        }

        if (type is IArrayTypeSymbol array)
        {
            return IsFileType(array.ElementType);
        }

        var enumerable = type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
            ? (INamedTypeSymbol)type
            : type.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, enumerableOfT));
        return enumerable is not null && IsFileType(enumerable.TypeArguments[0]);
    }

    private void ValidateParameters(IMethodSymbol method, string methodDisplay, string path, List<ParameterModel> parameters)
    {
        if (parameters.Count(p => p.Kind == ParameterKind.Body) > 1)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.MultipleBodies, method, methodDisplay));
        }

        if (parameters.Any(p => p.Kind == ParameterKind.Body) && parameters.Any(p => p.Kind == ParameterKind.Form))
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.BodyWithForm, method, methodDisplay));
        }

        if (parameters.Count(p => p.Kind == ParameterKind.CancellationToken) > 1)
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.MultipleCancellationTokens, method, methodDisplay));
        }

        var variables = PathVariable.Matches(path).Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
        var pathParameters = parameters.Where(p => p.Kind == ParameterKind.Path).ToList();

        foreach (var variable in variables.Where(v => pathParameters.All(p => p.Key != v)))
        {
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.UnboundPathVariable, method, path, methodDisplay, variable));
        }

        foreach (var parameter in pathParameters.Where(p => !variables.Contains(p.Key)))
        {
            var symbol = method.Parameters.FirstOrDefault(p => Identifier(p.Name) == parameter.Identifier);
            diagnostics.Add(DiagnosticInfo.Create(Diagnostics.UnknownPathParameter, symbol ?? (ISymbol)method, parameter.Identifier.TrimStart('@'), methodDisplay, parameter.Key, path));
        }
    }
}
