using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyPeasy.SourceGenerator;

/// <summary>
/// Generates an <see cref="HttpClient"/>-based implementation for every interface whose methods carry
/// EasyPeasy HTTP method attributes, and registers it so <c>EasyPeasyClient.Create&lt;T&gt;()</c> can find it.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ClientGenerator : IIncrementalGenerator
{
    internal const string InterfacesStep = "EasyPeasy.Interfaces";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (syntaxContext, cancellationToken) =>
                    syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node, cancellationToken) is INamedTypeSymbol symbol
                        ? Parser.TryParse(symbol, syntaxContext.SemanticModel.Compilation, cancellationToken)
                        : null)
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(InterfacesStep);

        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.AssemblyName ?? "Assembly");

        var all = interfaces.Collect().Combine(assemblyName);

        context.RegisterSourceOutput(all, static (output, source) =>
        {
            var (models, assembly) = source;

            // A partial interface declared in several files is parsed once per declaration.
            var distinct = models
                .GroupBy(m => m.FullName, StringComparer.Ordinal)
                .Select(g => g.First())
                .OrderBy(m => m.FullName, StringComparer.Ordinal)
                .ToList();

            // Problems in a base interface are found again for each interface that inherits it.
            foreach (var diagnostic in distinct.SelectMany(m => m.Diagnostics).Distinct())
            {
                output.ReportDiagnostic(diagnostic.ToDiagnostic());
            }

            var valid = distinct.Where(m => !m.HasErrors).ToList();
            if (valid.Count > 0)
            {
                output.AddSource("EasyPeasy.Clients.g.cs", Emitter.Emit(assembly, valid));
            }
        });
    }
}
