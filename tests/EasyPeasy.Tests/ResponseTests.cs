using System.Net;
using System.Text;
using EasyPeasy.Tests.Apis;
using EasyPeasy.Tests.Infrastructure;

namespace EasyPeasy.Tests;

public class ResponseTests
{
    private static ICustomerApi Customers(StubHttpHandler handler) => EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient());

    [Fact]
    public async Task Json_responses_are_deserialized()
    {
        var api = Customers(StubHttpHandler.Json("""{"id":3,"name":"Cat"}"""));

        Assert.Equal(new Customer(3, "Cat"), await api.GetAsync(3));
    }

    [Fact]
    public async Task Json_collections_are_deserialized()
    {
        var api = Customers(StubHttpHandler.Json("""[{"id":1,"name":"A"},{"id":2,"name":"B"}]"""));

        var customers = await api.SearchAsync();

        Assert.Equal([new Customer(1, "A"), new Customer(2, "B")], customers);
    }

    [Fact]
    public async Task Structured_json_media_types_use_the_json_handler()
    {
        var api = Customers(StubHttpHandler.Json("""{"id":3,"name":"Cat"}""", mediaType: "application/vnd.customer+json"));

        Assert.Equal("Cat", (await api.GetAsync(3)).Name);
    }

    [Fact]
    public async Task No_content_returns_null()
    {
        var api = Customers(new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent)));

        Assert.Null(await api.FindAsync(1));
    }

    [Fact]
    public async Task Missing_content_type_falls_back_to_Consumes()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("""{"id":1,"name":"A"}""")),
        });

        Assert.Equal("A", (await Customers(handler).GetAsync(1)).Name);
    }

    [Fact]
    public async Task Plain_text_is_returned_as_is_and_converted_to_primitives()
    {
        var text = EasyPeasyClient.Create<ITextApi>(StubHttpHandler.Text("hello \"world\"").CreateClient());
        Assert.Equal("hello \"world\"", await text.GetTextAsync());

        var number = EasyPeasyClient.Create<ITextApi>(StubHttpHandler.Text(" 42\n").CreateClient());
        Assert.Equal(42, await number.GetNumberAsync());

        var empty = EasyPeasyClient.Create<ITextApi>(StubHttpHandler.Text(string.Empty).CreateClient());
        Assert.Null(await empty.GetOptionalNumberAsync());
    }

    [Fact]
    public async Task Other_text_types_use_the_text_handler()
    {
        var api = EasyPeasyClient.Create<ITextApi>(StubHttpHandler.Text("a,b", mediaType: "text/csv").CreateClient());

        Assert.Equal("a,b", await api.GetTextAsync());
    }

    [Fact]
    public async Task Byte_arrays_are_read_regardless_of_media_type()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1, 2, 3]) { Headers = { ContentType = new("image/png") } },
        });

        Assert.Equal([1, 2, 3], await Customers(handler).PhotoAsync(1));
    }

    [Fact]
    public async Task Streams_are_returned_unread()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([4, 5]) });

        await using var stream = await Customers(handler).PhotoStreamAsync(1);
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);

        Assert.Equal([4, 5], copy.ToArray());
    }

    [Fact]
    public async Task Stream_results_throw_for_error_status()
    {
        var api = Customers(StubHttpHandler.Text("gone", HttpStatusCode.Gone));

        var error = await Assert.ThrowsAsync<ApiException>(() => api.PhotoStreamAsync(1));

        Assert.Equal(HttpStatusCode.Gone, error.StatusCode);
    }

    [Fact]
    public async Task HttpResponseMessage_results_are_returned_without_checking_the_status()
    {
        var api = Customers(StubHttpHandler.Text("nope", HttpStatusCode.NotFound));

        using var response = await api.HeadAsync(1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiResponse_reports_success_with_content_and_headers()
    {
        var handler = new StubHttpHandler(_ =>
        {
            var response = StubHttpHandler.Respond(HttpStatusCode.OK, """{"id":1,"name":"A"}""", "application/json");
            response.Headers.ETag = new("\"v1\"");
            return response;
        });

        var result = await Customers(handler).TryGetAsync(1);

        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(new Customer(1, "A"), result.Content);
        Assert.Equal("\"v1\"", result.Headers.ETag?.Tag);
        Assert.Equal("application/json", result.ContentHeaders.ContentType?.MediaType);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ApiResponse_reports_failure_without_throwing()
    {
        var api = Customers(StubHttpHandler.Json("""{"error":"missing"}""", HttpStatusCode.NotFound));

        var result = await api.TryGetAsync(1);

        Assert.False(result.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Content);
        Assert.Equal("""{"error":"missing"}""", result.Error.Content);
        Assert.Throws<ApiException>(() => result.EnsureSuccessStatusCode());
    }

    [Fact]
    public async Task Untyped_ApiResponse_reports_the_status()
    {
        var ok = await Customers(new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted))).TryDeleteAsync(1);
        Assert.True(ok.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Accepted, ok.StatusCode);

        var failed = await Customers(StubHttpHandler.Text("locked", HttpStatusCode.Locked)).TryDeleteAsync(1);
        Assert.False(failed.IsSuccessStatusCode);
        Assert.Equal("locked", failed.Error.Content);
    }

    [Fact]
    public async Task ValueTask_methods_work()
    {
        var handler = StubHttpHandler.Json("""{"id":9,"name":"V"}""");
        var api = Customers(handler);

        Assert.Equal(9, (await api.GetValueTaskAsync(9)).Id);
        await api.DeleteValueTaskAsync(9);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
    }

    [Fact]
    public async Task Async_enumerables_stream_json_arrays()
    {
        var api = Customers(StubHttpHandler.Json("""[{"id":1,"name":"A"},{"id":2,"name":"B"},{"id":3,"name":"C"}]"""));

        var names = new List<string>();
        await foreach (var customer in api.StreamAsync())
        {
            names.Add(customer.Name);
        }

        Assert.Equal(["A", "B", "C"], names);
    }

    [Fact]
    public async Task Async_enumerables_throw_for_error_status()
    {
        var api = Customers(StubHttpHandler.Json("[]", HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ApiException>(async () =>
        {
            await foreach (var customer in api.StreamAsync())
            {
                Assert.Fail("No items expected");
            }
        });
    }

    [Fact]
    public async Task A_response_media_type_with_no_handler_is_reported()
    {
        var api = Customers(StubHttpHandler.Text("<a/>", mediaType: "application/xml"));

        var error = await Assert.ThrowsAsync<EasyPeasyException>(() => api.GetAsync(1));

        Assert.Contains("application/xml", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Error_messages_describe_the_request()
    {
        var api = Customers(StubHttpHandler.Text("boom", HttpStatusCode.InternalServerError));

        var error = await Assert.ThrowsAsync<ApiException>(() => api.DeleteAsync(4));

        Assert.Equal(
            "Response status code does not indicate success: 500 (Internal Server Error). Request: DELETE https://api.test/customers/4",
            error.Message);
    }
}
