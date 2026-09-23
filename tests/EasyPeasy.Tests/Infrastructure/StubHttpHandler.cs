using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EasyPeasy.Tests.Infrastructure;

/// <summary>
/// An in-memory HTTP handler: records every request (including its body, read before EasyPeasy disposes it)
/// and answers with a canned response.
/// </summary>
public sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public StubHttpHandler()
        : this(_ => new HttpResponseMessage(HttpStatusCode.OK))
    {
    }

    public List<RecordedRequest> Requests { get; } = [];

    public RecordedRequest LastRequest => Requests[^1];

    public static StubHttpHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK, string mediaType = "application/json") =>
        new(_ => Respond(status, json, mediaType));

    public static StubHttpHandler Text(string text, HttpStatusCode status = HttpStatusCode.OK, string mediaType = "text/plain") =>
        new(_ => Respond(status, text, mediaType));

    public static HttpResponseMessage Respond(HttpStatusCode status, string body, string mediaType) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };

    public HttpClient CreateClient(string baseAddress = "https://api.test/") =>
        new(this) { BaseAddress = new Uri(baseAddress) };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Requests.Add(await RecordedRequest.CaptureAsync(request));
        var response = respond(request);
        response.RequestMessage ??= request;
        return response;
    }
}

public sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    Dictionary<string, string[]> Headers,
    MediaTypeHeaderValue? ContentType,
    byte[]? Body)
{
    public string? BodyText => Body is null ? null : Encoding.UTF8.GetString(Body);

    public string? Header(string name) =>
        Headers.TryGetValue(name, out var values) ? string.Join(", ", values) : null;

    public static async Task<RecordedRequest> CaptureAsync(HttpRequestMessage request)
    {
        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in request.Headers)
        {
            headers[header.Key] = [.. header.Value];
        }

        byte[]? body = null;
        if (request.Content is not null)
        {
            body = await request.Content.ReadAsByteArrayAsync();
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = [.. header.Value];
            }
        }

        return new RecordedRequest(request.Method, request.RequestUri!, headers, request.Content?.Headers.ContentType, body);
    }
}
