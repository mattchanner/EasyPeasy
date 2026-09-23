EasyPeasy
=========

[![CI](https://github.com/mattchanner/EasyPeasy/actions/workflows/ci.yml/badge.svg)](https://github.com/mattchanner/EasyPeasy/actions/workflows/ci.yml)

EasyPeasy gets rid of the boilerplate needed to call REST APIs. Describe the API as a C# interface,
annotate it with [JAX-RS](http://en.wikipedia.org/wiki/Java_API_for_RESTful_Web_Services) style attributes,
and EasyPeasy generates an `HttpClient`-based implementation at compile time.

- **Compile-time generation.** A Roslyn source generator writes the client, so mistakes such as a
  `{variable}` with no matching parameter are build errors. The code is plain C# you can step into.
- **Native AOT and trimming.** No reflection or runtime code generation. Pair it with a
  `JsonSerializerContext` and the client publishes with `PublishAot`.
- **Built on `HttpClient`.** Works with `IHttpClientFactory`, `DelegatingHandler`s, resilience handlers and
  OpenTelemetry, because it is an ordinary `HttpClient`.
- **Modern .NET.** System.Text.Json, `CancellationToken`, `ValueTask`, `IAsyncEnumerable<T>` streaming,
  multipart uploads, and nullable annotations.

## Installation

```
dotnet add package EasyPeasy
dotnet add package EasyPeasy.Extensions.DependencyInjection   # for IHttpClientFactory
```

Optional add-ons: `EasyPeasy.Xml` (DataContractSerializer XML) and `EasyPeasy.SkiaSharp` (image bodies).

## Example

```csharp
using EasyPeasy;
using EasyPeasy.Attributes;

public sealed record Customer(int Id, string Name);

[Path("/customers")]
public interface ICustomerApi
{
    [GET("/{id}")]
    Task<Customer> GetAsync([PathParam] int id, CancellationToken cancellationToken = default);

    [GET]
    Task<List<Customer>> SearchAsync([QueryParam] string? name = null, [QueryParam("tag")] string[]? tags = null);

    // A parameter without a binding attribute is the request body (JSON by default).
    [POST]
    Task<Customer> CreateAsync(Customer customer);

    [PATCH("/{id}")]
    Task RenameAsync([PathParam] int id, [FormParam] string name);

    [DELETE("/{id}")]
    Task DeleteAsync([PathParam] int id, [HeaderParam("If-Match")] string? etag = null);

    // Report 4xx/5xx through the result rather than throwing.
    [GET("/{id}")]
    Task<ApiResponse<Customer>> TryGetAsync([PathParam] int id);

    // Items are yielded while the JSON array is still arriving.
    [GET("/export")]
    IAsyncEnumerable<Customer> ExportAsync(CancellationToken cancellationToken = default);

    // FileInfo and FilePart parameters make the request multipart/form-data.
    [POST("/{id}/documents")]
    Task UploadAsync([PathParam] int id, FilePart document, [FormParam] string? description = null);
}
```

### With dependency injection

```csharp
builder.Services
    .AddEasyPeasyClient<ICustomerApi>(new Uri("https://api.example.com/v1/"))
    .AddHttpMessageHandler<AuthHandler>()          // any DelegatingHandler
    .AddStandardResilienceHandler();               // Microsoft.Extensions.Http.Resilience

// Inject ICustomerApi anywhere:
public sealed class CustomerService(ICustomerApi customers) { ... }
```

### Without dependency injection

```csharp
// Uses a shared, pooled HttpClient.
var customers = EasyPeasyClient.Create<ICustomerApi>("https://api.example.com/v1/");

// Or bring your own HttpClient (EasyPeasy does not dispose it).
var customers = EasyPeasyClient.Create<ICustomerApi>(httpClient);
```

## How requests are built

| Attribute | Applies to | Effect |
|---|---|---|
| `[GET]`, `[POST]`, `[PUT]`, `[PATCH]`, `[DELETE]`, `[HEAD]`, `[OPTIONS]` | method | HTTP method, with an optional path: `[GET("/{id}")]` |
| `[Path("/x")]` | interface, method | Path. Interface path, then method path, are appended to the base address, **including any path in the base address** |
| `[PathParam]` | parameter | Fills a `{name}` in the path. Values are escaped, so `a/../b` stays one segment |
| `[QueryParam]` | parameter | Query string value. `null` is left out; arrays and lists repeat the key |
| `[HeaderParam("X")]` | parameter | Request header. `null` is left out. `Content-Type` and other content headers apply to the body |
| `[FormParam]` | parameter | `application/x-www-form-urlencoded` field, or a multipart part |
| `[Body]` | parameter | Explicitly marks the body (optional: an unannotated parameter is the body) |
| `[Header("X", "v")]` | interface, method | Fixed header. Method headers replace interface headers; `[HeaderParam]` replaces both |
| `[Multipart]` | method | Send `[FormParam]`s as `multipart/form-data` (automatic when a parameter is `FileInfo` or `FilePart`) |
| `[Consumes("type")]` | interface, method | `Accept` header; also picks the reader when a response has no `Content-Type` |
| `[Produces("type")]` | interface, method | Body serializer and `Content-Type` |

A `CancellationToken` parameter is passed to `HttpClient.SendAsync`. The parameter name is used when a binding
attribute has no name. Values are formatted with the invariant culture: `true`/`false`, ISO 8601 dates, enum names.
Replace `EasyPeasySettings.UrlParameterFormatter` to change this.

## Return types

| Return type | Behaviour |
|---|---|
| `Task`, `ValueTask` | Throws `ApiException` for a non-2xx status |
| `Task<T>`, `ValueTask<T>` | Deserializes the body using the **response's** `Content-Type` (falling back to `[Consumes]`). `204 No Content` gives `default` |
| `Task<ApiResponse>`, `Task<ApiResponse<T>>` | Never throws for status codes; check `IsSuccessStatusCode`, `Content`, `Error`, `Headers` |
| `IAsyncEnumerable<T>` | Streams a JSON array item by item |
| `Task<byte[]>` | The raw body |
| `Task<Stream>` | The unread body stream. Dispose it |
| `Task<HttpResponseMessage>` | The raw response, with no status check. Dispose it |

`ApiException` derives from `HttpRequestException` and carries `StatusCode`, `Content` (the body text), `Headers`,
`Method` and `RequestUri`. Network failures and timeouts are not wrapped: they surface as the `HttpRequestException` or
`TaskCanceledException` that `HttpClient` throws.

## Serialization

JSON uses System.Text.Json with `JsonSerializerDefaults.Web` (camelCase, case-insensitive). `text/*` bodies are read as
strings and converted to primitives, so `text/plain` `42` can be read as an `int`. `+json` and `+xml` media types
use the JSON and XML handlers.

```csharp
var settings = new EasyPeasySettings
{
    // Custom JSON options, or a JsonSerializerContext for trimming and Native AOT:
    MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault(
        new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = AppJsonContext.Default }),
};

settings.MediaTypeHandlers.AddXml();              // EasyPeasy.Xml
settings.MediaTypeHandlers.AddSkiaSharpImages();  // EasyPeasy.SkiaSharp
settings.MediaTypeHandlers.Register("application/x-msgpack", new MyMessagePackHandler());  // any IMediaTypeHandler

services.AddEasyPeasyClient<ICustomerApi>(s => s.MediaTypeHandlers = settings.MediaTypeHandlers);
```

## Compile-time diagnostics

| Id | Problem |
|---|---|
| EP0001 | Method has no HTTP method attribute |
| EP0002 | Interface declares a property or event |
| EP0003 / EP0004 | Generic method / generic interface |
| EP0005 | Return type isn't `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` or `IAsyncEnumerable<T>` |
| EP0006 | `ref`, `out` or `in` parameter |
| EP0007 / EP0008 | More than one body / body combined with form fields |
| EP0009 / EP0010 | `{variable}` with no `[PathParam]` / `[PathParam]` not in the path |
| EP0011 | Parameter has more than one binding attribute |
| EP0012 | Interface is private or protected |
| EP0013 / EP0014 | More than one HTTP method attribute / `CancellationToken` |

## Upgrading from 2.x

3.0 replaces the Reflection.Emit runtime with a source generator, and custom plumbing with standard `HttpClient`
features.

| 2.x | 3.0 |
|---|---|
| `new EasyPeasyFactory().Create<T>(uri)` | `EasyPeasyClient.Create<T>(uri)` or `services.AddEasyPeasyClient<T>(uri)` |
| `ICredentials` argument (was ignored) | `HttpClientHandler.Credentials`, or a `DelegatingHandler` that sets `Authorization` |
| `IRequestInterceptor`, `BeforeSend`/`ResponseReceived` events | A `DelegatingHandler` |
| `IServiceClient.Timeout` | `HttpClient.Timeout`, or `AddStandardResilienceHandler()` |
| `HttpRequestException` from `EnsureSuccessStatusCode` | `ApiException` (still an `HttpRequestException`) with status and body |
| `Task<IHttpResponse>` | `Task<HttpResponseMessage>` or `Task<ApiResponse<T>>` |
| Newtonsoft.Json, PascalCase bodies | System.Text.Json, camelCase bodies (pass `JsonSerializerOptions` to change) |
| Default media type `text/xml` | `application/json`. Add `EasyPeasy.Xml` and `[Consumes]`/`[Produces]` for XML |
| `IMediaTypeHandler.ReadObject`/`WriteObject` (sync) | `DeserializeAsync`/`Serialize` over `HttpContent` |
| Images via SkiaSharp in the core package | `byte[]`/`Stream` in core, or the `EasyPeasy.SkiaSharp` add-on |
| MEF `[Export]` | Dependency injection |
| `[PathParam("name")]` required | `[PathParam]` uses the parameter name; `[GET("/path")]` shorthand |
| `Task<int>` read as raw binary | Read with the response media type (`42` in JSON or text) |

Bugs fixed in 3.0: the base address path was dropped; interfaces with the same name in different namespaces
collided; path parameters could add path segments; inherited interface methods failed with `TypeLoadException`;
multipart uploads lost their boundary; network failures skipped interceptors; each client created its own
`HttpClient` and never disposed responses.

## Building

```
dotnet test
dotnet pack -c Release -o packages
```

The `examples` folder has a small ASP.NET Core server and a client that uses it. Run
`dotnet run --project examples/EasyPeasy.Example.Server`, then `dotnet run --project examples/EasyPeasy.Example`.

## License

[MIT](LICENSE)
