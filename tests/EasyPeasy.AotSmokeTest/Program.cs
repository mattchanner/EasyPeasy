using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyPeasy;
using EasyPeasy.Attributes;
using Microsoft.Extensions.DependencyInjection;

// Exercises the generated client, JSON source generation and DI under Native AOT.
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = SmokeJsonContext.Default };

var services = new ServiceCollection();
services
    .AddEasyPeasyClient<ITodoApi>(new Uri("https://aot.test/api/"), settings =>
        settings.MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault(jsonOptions))
    .ConfigurePrimaryHttpMessageHandler(() => new StubHandler());

await using var provider = services.BuildServiceProvider();
var api = provider.GetRequiredService<ITodoApi>();

var todo = await api.GetAsync(1);
Check(todo is { Id: 1, Title: "Write tests" }, "GET returned the todo");

var created = await api.CreateAsync(new Todo(0, "Ship it"));
Check(created.Title == "Ship it", "POST round-tripped the body");

var titles = new List<string>();
await foreach (var item in api.StreamAsync())
{
    titles.Add(item.Title);
}

Check(titles.SequenceEqual(["a", "b"]), "IAsyncEnumerable streamed the array");

var missing = await api.TryGetAsync(404);
Check(!missing.IsSuccessStatusCode && missing.StatusCode == HttpStatusCode.NotFound, "ApiResponse reported the 404");

Console.WriteLine("AOT smoke test passed.");
return 0;

static void Check(bool condition, string description)
{
    Console.WriteLine($"{(condition ? "ok  " : "FAIL")} {description}");
    if (!condition)
    {
        Environment.Exit(1);
    }
}

public sealed record Todo(int Id, string Title);

[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(Todo[]))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;

[Path("/todos")]
public interface ITodoApi
{
    [GET("/{id}")]
    Task<Todo> GetAsync([PathParam] int id, CancellationToken cancellationToken = default);

    [GET("/{id}")]
    Task<ApiResponse<Todo>> TryGetAsync([PathParam] int id);

    [POST]
    Task<Todo> CreateAsync(Todo todo);

    [GET("/stream")]
    IAsyncEnumerable<Todo> StreamAsync();
}

internal sealed class StubHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath;
        string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        (HttpStatusCode status, string json) = (request.Method.Method, path) switch
        {
            ("GET", "/api/todos/1") => (HttpStatusCode.OK, """{"id":1,"title":"Write tests"}"""),
            ("GET", "/api/todos/stream") => (HttpStatusCode.OK, """[{"id":1,"title":"a"},{"id":2,"title":"b"}]"""),
            ("POST", "/api/todos") => (HttpStatusCode.Created, body!),
            _ => (HttpStatusCode.NotFound, """{"title":"Not Found"}"""),
        };

        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
