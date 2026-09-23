using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasyPeasy.SourceGenerator.Tests;

/// <summary>Compiles source against EasyPeasy and runs the generator over it.</summary>
internal static class GeneratorHarness
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> References = new(() =>
    {
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(System.IO.Path.PathSeparator).ToList();
        paths.Add(typeof(EasyPeasyClient).Assembly.Location);
        return [.. paths.Distinct().Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))];
    });

    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    public static CSharpCompilation Compile(params string[] sources) =>
        CSharpCompilation.Create(
            "GeneratorTestAssembly",
            sources.Select(s => CSharpSyntaxTree.ParseText(s, ParseOptions)),
            References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    public static GeneratorResult Run(string source)
    {
        var compilation = Compile(Usings + source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ClientGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);

        var generated = driver.GetRunResult().GeneratedTrees.SingleOrDefault()?.ToString();
        return new GeneratorResult(generatorDiagnostics, output.GetDiagnostics(), generated);
    }

    public const string Usings = """
        using System.Collections.Generic;
        using System.IO;
        using System.Net.Http;
        using System.Threading;
        using System.Threading.Tasks;
        using EasyPeasy;
        using EasyPeasy.Attributes;

        """;
}

internal sealed record GeneratorResult(
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics,
    string? GeneratedSource)
{
    public IEnumerable<string> Ids => GeneratorDiagnostics.Select(d => d.Id);
}
