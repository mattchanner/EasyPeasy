using System.Text.Json;
using System.Text.Json.Serialization;
using EasyPeasy.Tests.Apis;
using EasyPeasy.Tests.Infrastructure;

namespace EasyPeasy.Tests;

public class ClientCreationTests
{
    [Fact]
    public async Task A_client_without_a_base_address_explains_how_to_set_one()
    {
        var api = EasyPeasyClient.Create<ICustomerApi>(new HttpClient(new StubHttpHandler()));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => api.DeleteAsync(1));

        Assert.Contains("BaseAddress", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Interfaces_without_a_generated_client_are_reported()
    {
        var error = Assert.Throws<EasyPeasyException>(() => EasyPeasyClient.Create<INotAnEasyPeasyApi>(new HttpClient()));

        Assert.Contains(nameof(INotAnEasyPeasyApi), error.Message, StringComparison.Ordinal);
        Assert.Contains("[GET]", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Clients_can_be_created_from_a_base_address()
    {
        var fromUri = EasyPeasyClient.Create<ICustomerApi>(new Uri("https://api.test/"));
        var fromString = EasyPeasyClient.Create<ICustomerApi>("https://api.test/");

        Assert.NotNull(fromUri);
        Assert.NotNull(fromString);
    }

    [Fact]
    public void Relative_base_addresses_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => EasyPeasyClient.Create<ICustomerApi>(new Uri("/api", UriKind.Relative)));
    }

    [Fact]
    public async Task Json_source_generation_contexts_are_supported()
    {
        // This is the Native AOT configuration: every type comes from a JsonSerializerContext.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = TestJsonContext.Default };
        var settings = new EasyPeasySettings { MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault(options) };
        var handler = StubHttpHandler.Json("""{"id":1,"name":"A"}""");
        var api = EasyPeasyClient.Create<ICustomerApi>(handler.CreateClient(), settings);

        Assert.Equal(new Customer(1, "A"), await api.GetAsync(1));

        // List<Customer> is not in the context, which proves the context (not reflection) is being used.
        await Assert.ThrowsAsync<NotSupportedException>(() => api.SearchAsync());
    }
}

[JsonSerializable(typeof(Customer))]
internal sealed partial class TestJsonContext : JsonSerializerContext;
