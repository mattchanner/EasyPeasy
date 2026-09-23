using System.Net;
using System.Text;
using EasyPeasy.Tests.Apis;
using EasyPeasy.Tests.Infrastructure;

namespace EasyPeasy.Tests;

/// <summary>
/// One test per bug found in the 2.x review. Each failed against 2.0.0.
/// </summary>
public class RegressionTests
{
    [Theory]
    [InlineData("https://api.test/api/")]
    [InlineData("https://api.test/api")]
    public async Task Bug1_base_address_path_is_kept(string baseAddress)
    {
        var handler = StubHttpHandler.Json("""{"id":1,"name":"Ann"}""");
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient(baseAddress));

        await api.GetAsync(1);

        Assert.Equal("https://api.test/api/customers/1", handler.LastRequest.Uri.ToString());
    }

    [Fact]
    public async Task Bug2_interfaces_with_the_same_name_in_different_namespaces_get_their_own_client()
    {
        var handler = StubHttpHandler.Json("\"ok\"");
        var first = EasyPeasyClient.Create<Apis.First.IDuplicateName>(handler.CreateClient());
        var second = EasyPeasyClient.Create<Apis.Second.IDuplicateName>(handler.CreateClient());

        await first.WhoAmIAsync();
        Assert.Equal("/first", handler.LastRequest.Uri.AbsolutePath);

        await second.WhoAmIAsync();
        Assert.Equal("/second", handler.LastRequest.Uri.AbsolutePath);
    }

    [Theory]
    [InlineData("a/../../admin#x", "/items/a%2F..%2F..%2Fadmin%23x")]
    [InlineData("../admin", "/items/..%2Fadmin")]
    [InlineData("a?b=c", "/items/a%3Fb%3Dc")]
    [InlineData("hello world", "/items/hello%20world")]
    public async Task Bug3_path_parameters_are_escaped_and_cannot_change_the_path(string id, string expectedPath)
    {
        var handler = new StubHttpHandler();
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient());

        await api.EscapedAsync(id);

        Assert.Equal(expectedPath, handler.LastRequest.Uri.AbsolutePath);
        Assert.Equal(string.Empty, handler.LastRequest.Uri.Query);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public async Task Bug3_dot_segments_are_rejected(string id)
    {
        // System.Uri decodes %2E and removes dot segments, so these cannot be sent safely at all.
        var handler = new StubHttpHandler();
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => api.EscapedAsync(id));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Bug4_value_types_are_read_using_the_response_media_type()
    {
        var handler = StubHttpHandler.Json("42");
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        Assert.Equal(42, await api.CountAsync());
    }

    [Fact]
    public async Task Bug4_json_strings_are_deserialized_rather_than_returned_raw()
    {
        var handler = StubHttpHandler.Json("\"ok\"");
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        Assert.Equal("ok", await api.NameAsync());
    }

    [Fact]
    public async Task Bug5_inherited_interface_methods_are_implemented()
    {
        var handler = StubHttpHandler.Json("\"ok\"");
        var api = EasyPeasyClient.Create<IDerivedApi>(handler.CreateClient());

        await api.PingAsync();
        Assert.Equal("/base/ping", handler.LastRequest.Uri.AbsolutePath);

        await api.PongAsync();
        Assert.Equal("/derived/pong", handler.LastRequest.Uri.AbsolutePath);
    }

    [Fact]
    public async Task Bug5_an_interface_that_only_inherits_methods_is_implemented()
    {
        var handler = StubHttpHandler.Json("\"ok\"");
        var api = EasyPeasyClient.Create<IEmptyDerivedApi>(handler.CreateClient());

        Assert.Equal("ok", await api.PingAsync());
    }

    [Fact]
    public async Task Bug6_credentials_are_applied_by_the_http_client_pipeline()
    {
        // 2.x accepted ICredentials and silently ignored them. 3.0 leaves authentication to the
        // HttpClient: a DelegatingHandler (or HttpClientHandler.Credentials) sees every request.
        var stub = new StubHttpHandler();
        var auth = new BearerTokenHandler("secret") { InnerHandler = stub };
        var api = EasyPeasyClient.Create<ICustomerApi>(new HttpClient(auth) { BaseAddress = new Uri("https://api.test/") });

        await api.DeleteAsync(1);

        Assert.Equal("Bearer secret", stub.LastRequest.Header("Authorization"));
    }

    [Fact]
    public async Task Bug7_multipart_uploads_keep_their_boundary_and_parts()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"easypeasy-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "file contents");
        try
        {
            var handler = new StubHttpHandler();
            var api = EasyPeasyClient.Create<IFormApi>(handler.CreateClient());

            await api.UploadFileAsync(new FileInfo(path), "my notes");

            var request = handler.LastRequest;
            Assert.Equal("multipart/form-data", request.ContentType?.MediaType);
            string? boundary = request.ContentType?.Parameters.Single(p => p.Name == "boundary").Value?.Trim('"');
            Assert.False(string.IsNullOrEmpty(boundary));

            string body = request.BodyText!;
            Assert.Contains("--" + boundary, body, StringComparison.Ordinal);
            Assert.Contains($"name=file; filename={System.IO.Path.GetFileName(path)}", body, StringComparison.Ordinal);
            Assert.Contains("Content-Type: text/plain", body, StringComparison.Ordinal);
            Assert.Contains("file contents", body, StringComparison.Ordinal);
            Assert.Contains("name=description", body, StringComparison.Ordinal);
            Assert.Contains("my notes", body, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Bug8_error_responses_keep_their_status_and_body()
    {
        var handler = StubHttpHandler.Json("""{"title":"Not here"}""", HttpStatusCode.NotFound, "application/problem+json");
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        var error = await Assert.ThrowsAsync<ApiException>(() => api.GetAsync(7));

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("""{"title":"Not here"}""", error.Content);
        Assert.Equal("application/problem+json", error.ContentHeaders?.ContentType?.MediaType);
        Assert.Equal(HttpMethod.Get, error.Method);
        Assert.Equal("https://api.test/customers/7", error.RequestUri?.ToString());
        Assert.IsAssignableFrom<HttpRequestException>(error);
    }

    [Fact]
    public async Task Bug8_network_failures_reach_the_http_pipeline_and_the_caller()
    {
        var failure = new HttpRequestException("connection refused");
        var recorder = new RecordingHandler { InnerHandler = new StubHttpHandler(_ => throw failure) };
        var api = EasyPeasyClient.Create<ICustomerApi>(new HttpClient(recorder) { BaseAddress = new Uri("https://api.test/") });

        var thrown = await Assert.ThrowsAsync<HttpRequestException>(() => api.GetAsync(1));

        Assert.Same(failure, thrown);
        Assert.Same(failure, recorder.Exception);
    }

    [Fact]
    public async Task Bug9_responses_are_disposed_after_they_are_read()
    {
        var content = new TrackingContent("""{"id":1,"name":"Ann"}""");
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        await api.GetAsync(1);

        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task Bug9_error_responses_are_disposed()
    {
        var content = new TrackingContent("nope");
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content });
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        await Assert.ThrowsAsync<ApiException>(() => api.DeleteAsync(1));

        Assert.True(content.IsDisposed);
    }

    [Fact]
    public async Task Bug9_the_supplied_http_client_is_used_and_not_disposed()
    {
        var handler = new StubHttpHandler();
        using var client = handler.CreateClient();
        var api = EasyPeasyClient.Create<ICustomerApi>(client);

        await api.DeleteAsync(1);
        await api.DeleteAsync(2);

        Assert.Equal(2, handler.Requests.Count);
        using var response = await client.GetAsync(new Uri("https://api.test/still-usable"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class BearerTokenHandler(string token) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new("Bearer", token);
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class RecordingHandler : DelegatingHandler
    {
        public Exception? Exception { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Exception = ex;
                throw;
            }
        }
    }

    private sealed class TrackingContent(string text) : HttpContent
    {
        private readonly byte[] bytes = Encoding.UTF8.GetBytes(text);

        public bool IsDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(bytes, 0, bytes.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = bytes.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
