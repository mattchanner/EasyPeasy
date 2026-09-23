using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasyPeasy.SourceGenerator.Tests;

public class GeneratorTests
{
    [Fact]
    public void Valid_interfaces_generate_code_that_compiles_without_warnings()
    {
        var result = GeneratorHarness.Run("""
            namespace Sample;

            public sealed record Item(int Id, string? Name);

            [Path("/items"), Header("X-Version", "1")]
            public interface IItemApi
            {
                [GET("/{id}")] Task<Item?> GetAsync([PathParam] int id, CancellationToken cancellationToken = default);
                [GET] Task<List<Item>> SearchAsync([QueryParam] string? name, [QueryParam("tag")] string[]? tags);
                [POST] Task<ApiResponse<Item>> CreateAsync([Body] Item item);
                [PUT("/{id}")] ValueTask UpdateAsync([PathParam("id")] int @int, Item item);
                [DELETE("/{id}")] Task<ApiResponse> DeleteAsync([PathParam] int id);
                [GET("/stream")] IAsyncEnumerable<Item> StreamAsync(CancellationToken cancellationToken);
                [POST("/upload")] Task UploadAsync(FileInfo file, [FormParam] string? note, [HeaderParam("X-Trace")] string? trace);
                [PATCH("/{id}")] ValueTask<Item> PatchAsync([PathParam] int id, [QueryParam] bool? force);
                [HEAD] Task<HttpResponseMessage> HeadAsync();

                // Overloads get their own templates.
                [GET("/{id}")] Task<Item> GetAsync([PathParam] string id);

                // Default interface methods need no generated code.
                Task<Item?> GetFirstAsync() => GetAsync(1);
            }
            """);

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Empty(result.CompilationDiagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning));
        Assert.NotNull(result.GeneratedSource);
        Assert.Contains("ProxyRegistry.Register<global::Sample.IItemApi>", result.GeneratedSource, StringComparison.Ordinal);
        Assert.Contains("\"/items/{id}\"", result.GeneratedSource, StringComparison.Ordinal);
        Assert.Contains("isMultipart: true", result.GeneratedSource, StringComparison.Ordinal);
        Assert.Contains("__request.AddPathParameter(\"id\", @int);", result.GeneratedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Interfaces_without_http_attributes_are_ignored()
    {
        var result = GeneratorHarness.Run("""
            public interface IPlain { Task DoAsync(); }
            """);

        Assert.Empty(result.GeneratorDiagnostics);
        Assert.Null(result.GeneratedSource);
    }

    [Fact]
    public void Partial_interfaces_are_generated_once()
    {
        var result = GeneratorHarness.Run("""
            public partial interface ISplit { [GET("/a")] Task AAsync(); }
            public partial interface ISplit { [GET("/b")] Task BAsync(); }
            """);

        Assert.Empty(result.CompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Equal(1, CountOccurrences(result.GeneratedSource!, "ProxyRegistry.Register<global::ISplit>"));
    }

    [Theory]
    [InlineData("EP0001", "[GET] Task A(); Task B();")]
    [InlineData("EP0002", "[GET] Task A(); string Name { get; }")]
    [InlineData("EP0003", "[GET] Task<T> A<T>();")]
    [InlineData("EP0005", "[GET] string A();")]
    [InlineData("EP0006", "[GET] Task A(out int x);")]
    [InlineData("EP0007", "[POST] Task A(string a, string b);")]
    [InlineData("EP0008", "[POST] Task A(string body, [FormParam] string field);")]
    [InlineData("EP0009", "[GET(\"/{id}\")] Task A();")]
    [InlineData("EP0010", "[GET(\"/x\")] Task A([PathParam] int id);")]
    [InlineData("EP0011", "[GET(\"/{id}\")] Task A([PathParam, QueryParam] int id);")]
    [InlineData("EP0013", "[GET, POST] Task A();")]
    [InlineData("EP0014", "[GET] Task A(CancellationToken a, CancellationToken b);")]
    public void Invalid_methods_are_reported(string expectedId, string members)
    {
        var result = GeneratorHarness.Run($"public interface IApi {{ {members} }}");

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal(expectedId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.Null(result.GeneratedSource);
    }

    [Fact]
    public void Generic_interfaces_are_reported()
    {
        var result = GeneratorHarness.Run("public interface IApi<T> { [GET] Task<T> A(); }");

        Assert.Equal(["EP0004"], result.Ids);
    }

    [Fact]
    public void Private_nested_interfaces_are_reported()
    {
        var result = GeneratorHarness.Run("public class Outer { private interface IApi { [GET] Task A(); } }");

        Assert.Equal(["EP0012"], result.Ids);
    }

    [Fact]
    public void Problems_in_a_base_interface_are_reported_once()
    {
        var result = GeneratorHarness.Run("""
            public interface IBase { [GET] Task A(); Task B(); }
            public interface IOne : IBase { }
            public interface ITwo : IBase { }
            """);

        Assert.Equal(["EP0001"], result.Ids);
    }

    [Fact]
    public void Valid_interfaces_are_still_generated_when_another_has_errors()
    {
        var result = GeneratorHarness.Run("""
            public interface IGood { [GET] Task A(); }
            public interface IBad { [GET] Task A(); Task B(); }
            """);

        Assert.Equal(["EP0001"], result.Ids);
        Assert.Contains("global::IGood", result.GeneratedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("global::IBad", result.GeneratedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Unrelated_edits_reuse_cached_models()
    {
        var compilation = GeneratorHarness.Compile(GeneratorHarness.Usings + "public interface IApi { [GET] Task A(); }");
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new ClientGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);
        driver = driver.RunGenerators(compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Unrelated { }")));

        var outputs = driver.GetRunResult().Results.Single().TrackedSteps[ClientGenerator.InterfacesStep]
            .SelectMany(step => step.Outputs)
            .Select(output => output.Reason)
            .ToList();

        Assert.NotEmpty(outputs);
        Assert.All(outputs, reason => Assert.True(reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged, reason.ToString()));
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        for (int i = text.IndexOf(value, StringComparison.Ordinal); i >= 0; i = text.IndexOf(value, i + 1, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
