using System.Net;
using System.Text.Json;
using EasyPeasy.Tests.Apis;
using EasyPeasy.Tests.Infrastructure;

namespace EasyPeasy.Tests;

public class RequestTests
{
    private readonly StubHttpHandler handler = StubHttpHandler.Json("""{"id":1,"name":"Ann"}""");

    private ICustomerApi Customers => EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

    [Fact]
    public async Task Http_methods_are_taken_from_the_attributes()
    {
        await Customers.GetAsync(1);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);

        await Customers.CreateAsync(new Customer(0, "Ann"));
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);

        await Customers.UpdateAsync(1, new Customer(1, "Ann"));
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);

        await Customers.RenameAsync(1, "Bob");
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);

        await Customers.DeleteAsync(1);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);

        using (await Customers.HeadAsync(1))
        {
            Assert.Equal(HttpMethod.Head, handler.LastRequest.Method);
        }

        using (await Customers.OptionsAsync())
        {
            Assert.Equal(HttpMethod.Options, handler.LastRequest.Method);
        }
    }

    [Fact]
    public async Task Json_bodies_use_web_defaults()
    {
        await Customers.CreateAsync(new Customer(5, "Ann"));

        var request = handler.LastRequest;
        Assert.Equal("application/json", request.ContentType?.MediaType);
        Assert.Equal("utf-8", request.ContentType?.CharSet);
        Assert.Equal("""{"id":5,"name":"Ann"}""", request.BodyText);
    }

    [Fact]
    public async Task Accept_header_comes_from_Consumes()
    {
        await Customers.GetAsync(1);

        Assert.Equal("application/json", handler.LastRequest.Header("Accept"));
    }

    [Fact]
    public async Task Json_is_the_default_media_type()
    {
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient());

        await api.PostAsync(new Customer(1, "Ann"));

        Assert.Equal("application/json", handler.LastRequest.Header("Accept"));
        Assert.Equal("application/json", handler.LastRequest.ContentType?.MediaType);
        Assert.Equal("https://api.test/items", handler.LastRequest.Uri.ToString());
    }

    [Fact]
    public async Task A_method_with_no_path_uses_the_base_address()
    {
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient("https://api.test/v2/"));

        await api.RootAsync();

        Assert.Equal("https://api.test/v2/", handler.LastRequest.Uri.ToString());
    }

    [Fact]
    public async Task Query_parameters_are_escaped_expanded_and_skip_nulls()
    {
        var handler = StubHttpHandler.Json("[]");
        var customers = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        await customers.SearchAsync(name: "Ann & Bob", ids: [1, 2], active: true, since: new DateTimeOffset(2026, 9, 23, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(
            "?name=Ann%20%26%20Bob&ids=1&ids=2&active=true&since=2026-09-23T10%3A00%3A00.0000000%2B00%3A00",
            handler.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task Null_query_parameters_are_left_out()
    {
        var handler = StubHttpHandler.Json("[]");
        var customers = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

        await customers.SearchAsync();

        Assert.Equal("https://api.test/customers", handler.LastRequest.Uri.ToString());
    }

    [Fact]
    public async Task Query_values_are_formatted_with_the_invariant_culture()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        try
        {
            var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient());
            await api.QueryAsync("x", new DateOnly(2026, 1, 2), DayOfWeek.Friday, 1.5);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }

        Assert.Equal("?q=x&on=2026-01-02&day=Friday&ratio=1.5", handler.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task Empty_query_strings_are_sent()
    {
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient());

        await api.QueryAsync(string.Empty, null, null, null);

        Assert.Equal("?q=", handler.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task A_custom_parameter_formatter_is_used()
    {
        var settings = new EasyPeasySettings { UrlParameterFormatter = new UpperCaseFormatter() };
        var api = EasyPeasyClient.Create<IDefaultsApi>(handler.CreateClient(), settings);

        await api.QueryAsync("abc", null, null, null);

        Assert.Equal("?q=ABC", handler.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task Interface_headers_are_sent()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.InterfaceHeaderAsync();

        Assert.Equal("1", handler.LastRequest.Header("X-Api-Version"));
    }

    [Fact]
    public async Task Method_headers_replace_interface_headers()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.MethodHeadersAsync();

        Assert.Equal("2", handler.LastRequest.Header("X-Api-Version"));
        Assert.Equal("on", handler.LastRequest.Header("X-Trace"));
    }

    [Fact]
    public async Task Header_parameters_replace_fixed_headers_and_skip_nulls()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.ParameterHeadersAsync("3", authorization: null);
        Assert.Equal("3", handler.LastRequest.Header("X-Api-Version"));
        Assert.Null(handler.LastRequest.Header("Authorization"));

        await api.ParameterHeadersAsync("3", "Bearer abc");
        Assert.Equal("Bearer abc", handler.LastRequest.Header("Authorization"));
    }

    [Fact]
    public async Task Sequence_header_parameters_send_every_value()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.RepeatedHeaderAsync(["a", "b"]);

        Assert.Equal(["a", "b"], handler.LastRequest.Headers["X-Tag"]);
    }

    [Fact]
    public async Task A_content_type_header_parameter_applies_to_the_body()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.ContentTypeHeaderAsync("application/vnd.test+json", "hello");

        Assert.Equal("application/vnd.test+json", handler.LastRequest.ContentType?.MediaType);
    }

    [Fact]
    public async Task An_accept_header_parameter_replaces_the_default()
    {
        var api = EasyPeasyClient.Create<IHeaderApi>(handler.CreateClient());

        await api.AcceptHeaderAsync("text/csv");

        Assert.Equal("text/csv", handler.LastRequest.Header("Accept"));
    }

    [Fact]
    public async Task Form_parameters_are_url_encoded()
    {
        var api = EasyPeasyClient.Create<IFormApi>(handler.CreateClient());

        await api.UrlEncodedAsync("Ann Lee", ["x", "y&z"]);

        Assert.Equal("application/x-www-form-urlencoded", handler.LastRequest.ContentType?.MediaType);
        Assert.Equal("first_name=Ann+Lee&tags=x&tags=y%26z", handler.LastRequest.BodyText);
    }

    [Fact]
    public async Task Multipart_methods_send_each_field_as_a_part()
    {
        var api = EasyPeasyClient.Create<IFormApi>(handler.CreateClient());

        await api.MultipartFieldsAsync("Report", 3);

        Assert.Equal("multipart/form-data", handler.LastRequest.ContentType?.MediaType);
        string body = handler.LastRequest.BodyText!;
        Assert.Contains("name=title", body, StringComparison.Ordinal);
        Assert.Contains("Report", body, StringComparison.Ordinal);
        Assert.Contains("name=count", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task File_parts_use_their_name_and_inferred_content_type()
    {
        var api = EasyPeasyClient.Create<IFormApi>(handler.CreateClient());
        using var stream = new MemoryStream([1, 2, 3]);

        await api.UploadPartAsync(new FilePart(stream, "scan.pdf"));

        string body = handler.LastRequest.BodyText!;
        Assert.Contains("name=document; filename=scan.pdf", body, StringComparison.Ordinal);
        Assert.Contains("Content-Type: application/pdf", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Byte_array_bodies_are_sent_as_is_with_the_Produces_media_type()
    {
        await Customers.UploadPhotoAsync(1, [1, 2, 3]);

        Assert.Equal("image/png", handler.LastRequest.ContentType?.MediaType);
        Assert.Equal([1, 2, 3], handler.LastRequest.Body);
    }

    [Fact]
    public async Task Stream_bodies_are_streamed_as_is()
    {
        using var stream = new MemoryStream([9, 8, 7]);

        await Customers.UploadStreamAsync(1, stream);

        Assert.Equal("application/octet-stream", handler.LastRequest.ContentType?.MediaType);
        Assert.Equal([9, 8, 7], handler.LastRequest.Body);
    }

    [Fact]
    public async Task Custom_json_options_are_used_for_bodies()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper };
        var settings = new EasyPeasySettings { MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault(options) };
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient(), settings);

        await api.CreateAsync(new Customer(1, "Ann"));

        Assert.Equal("""{"ID":1,"NAME":"Ann"}""", handler.LastRequest.BodyText);
    }

    [Fact]
    public async Task Cancellation_tokens_are_passed_to_the_http_client()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Customers.GetAsync(1, cancellation.Token));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Text_bodies_are_sent_as_plain_text()
    {
        var api = EasyPeasyClient.Create<ITextApi>(handler.CreateClient());

        await api.SendTextAsync("hello");

        Assert.Equal("text/plain", handler.LastRequest.ContentType?.MediaType);
        Assert.Equal("hello", handler.LastRequest.BodyText);
    }

    [Fact]
    public async Task A_body_media_type_with_no_handler_is_reported()
    {
        var settings = new EasyPeasySettings();
        settings.MediaTypeHandlers.Remove(MediaType.ApplicationJson);
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient(), settings);

        var error = await Assert.ThrowsAsync<EasyPeasyException>(() => api.CreateAsync(new Customer(1, "Ann")));

        Assert.Contains("application/json", error.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Requests_go_through_delegating_handlers()
    {
        var stub = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var logging = new CountingHandler { InnerHandler = stub };
        var api = EasyPeasyClient.Create<ICustomerApi>(new HttpClient(logging) { BaseAddress = new Uri("https://api.test/") });

        await api.DeleteAsync(1);
        await api.DeleteAsync(2);

        Assert.Equal(2, logging.Count);
    }

    private sealed class UpperCaseFormatter : IUrlParameterFormatter
    {
        public string? Format(object? value) => DefaultUrlParameterFormatter.Instance.Format(value)?.ToUpperInvariant();
    }

    private sealed class CountingHandler : DelegatingHandler
    {
        public int Count { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
