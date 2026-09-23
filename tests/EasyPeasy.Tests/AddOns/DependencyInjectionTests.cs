using System.Net;
using EasyPeasy.Tests.Apis;
using EasyPeasy.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EasyPeasy.Tests.AddOns;

public class DependencyInjectionTests
{
    [Fact]
    public async Task Clients_are_resolved_through_IHttpClientFactory()
    {
        var stub = StubHttpHandler.Json("""{"id":1,"name":"A"}""");
        var services = new ServiceCollection();
        services
            .AddEasyPeasyClient<ICustomerApi>(new Uri("https://api.test/v1/"))
            .ConfigurePrimaryHttpMessageHandler(() => stub);

        await using var provider = services.BuildServiceProvider();
        var api = provider.GetRequiredService<ICustomerApi>();

        Assert.Equal("A", (await api.GetAsync(1)).Name);
        Assert.Equal("https://api.test/v1/customers/1", stub.LastRequest.Uri.ToString());
    }

    [Fact]
    public async Task Delegating_handlers_run_for_every_request()
    {
        var stub = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var services = new ServiceCollection();
        services.AddTransient<ApiKeyHandler>();
        services
            .AddEasyPeasyClient<ICustomerApi>(new Uri("https://api.test/"))
            .AddHttpMessageHandler<ApiKeyHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => stub);

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<ICustomerApi>().DeleteAsync(1);

        Assert.Equal("key-123", stub.LastRequest.Header("X-Api-Key"));
    }

    [Fact]
    public async Task Settings_can_be_configured_per_client()
    {
        var stub = StubHttpHandler.Json("\"ok\"");
        var services = new ServiceCollection();
        services
            .AddEasyPeasyClient<IHeaderApi>(settings => settings.DefaultMediaType = "application/vnd.test+json")
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.test/"))
            .ConfigurePrimaryHttpMessageHandler(() => stub);

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHeaderApi>().InterfaceHeaderAsync();

        Assert.Equal("application/vnd.test+json", stub.LastRequest.Header("Accept"));
    }

    [Fact]
    public async Task Settings_can_use_other_services()
    {
        var stub = new StubHttpHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IUrlParameterFormatter, ShoutingFormatter>();
        services
            .AddEasyPeasyClient<IDefaultsApi>((provider, settings) =>
                settings.UrlParameterFormatter = provider.GetRequiredService<IUrlParameterFormatter>())
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.test/"))
            .ConfigurePrimaryHttpMessageHandler(() => stub);

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IDefaultsApi>().QueryAsync("quiet", null, null, null);

        Assert.Equal("?q=QUIET", stub.LastRequest.Uri.Query);
    }

    [Fact]
    public async Task Each_client_gets_its_own_settings()
    {
        var customersStub = StubHttpHandler.Json("""{"id":1,"name":"A"}""");
        var headersStub = new StubHttpHandler();
        var services = new ServiceCollection();
        services
            .AddEasyPeasyClient<ICustomerApi>(new Uri("https://customers.test/"))
            .ConfigurePrimaryHttpMessageHandler(() => customersStub);
        services
            .AddEasyPeasyClient<IHeaderApi>(new Uri("https://headers.test/"), settings => settings.DefaultMediaType = "text/plain")
            .ConfigurePrimaryHttpMessageHandler(() => headersStub);

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<ICustomerApi>().GetAsync(1);
        await provider.GetRequiredService<IHeaderApi>().InterfaceHeaderAsync();

        Assert.Equal("customers.test", customersStub.LastRequest.Uri.Host);
        Assert.Equal("headers.test", headersStub.LastRequest.Uri.Host);
        Assert.Equal("text/plain", headersStub.LastRequest.Header("Accept"));
    }

    private sealed class ApiKeyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("X-Api-Key", "key-123");
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class ShoutingFormatter : IUrlParameterFormatter
    {
        public string? Format(object? value) => DefaultUrlParameterFormatter.Instance.Format(value)?.ToUpperInvariant();
    }
}
