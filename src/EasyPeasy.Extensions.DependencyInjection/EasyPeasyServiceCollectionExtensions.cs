using EasyPeasy;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers EasyPeasy clients with <see cref="IServiceCollection"/>.</summary>
public static class EasyPeasyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TClient"/> as a typed client backed by <c>IHttpClientFactory</c>.
    /// </summary>
    /// <typeparam name="TClient">The service interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureSettings">Configures the client's <see cref="EasyPeasySettings"/>.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/>, for setting the base address with <c>ConfigureHttpClient</c> and adding
    /// handlers such as <c>AddHttpMessageHandler</c> or <c>AddStandardResilienceHandler</c>.
    /// </returns>
    public static IHttpClientBuilder AddEasyPeasyClient<TClient>(
        this IServiceCollection services,
        Action<EasyPeasySettings>? configureSettings = null)
        where TClient : class =>
        services.AddEasyPeasyClient<TClient>(
            configureSettings is null ? null : (_, settings) => configureSettings(settings));

    /// <summary>
    /// Registers <typeparamref name="TClient"/> as a typed client backed by <c>IHttpClientFactory</c>.
    /// </summary>
    /// <typeparam name="TClient">The service interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureSettings">Configures the client's <see cref="EasyPeasySettings"/> using other services.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddEasyPeasyClient<TClient>(
        this IServiceCollection services,
        Action<IServiceProvider, EasyPeasySettings>? configureSettings)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        string name = typeof(TClient).FullName ?? typeof(TClient).Name;
        var options = services.AddOptions<EasyPeasySettings>(name);
        if (configureSettings is not null)
        {
            options.Configure<IServiceProvider>((settings, provider) => configureSettings(provider, settings));
        }

        return services
            .AddHttpClient(name)
            .AddTypedClient((httpClient, provider) =>
            {
                var settings = provider.GetRequiredService<IOptionsMonitor<EasyPeasySettings>>().Get(name);
                return EasyPeasyClient.Create<TClient>(httpClient, settings);
            });
    }

    /// <summary>
    /// Registers <typeparamref name="TClient"/> as a typed client backed by <c>IHttpClientFactory</c>, sending
    /// requests to <paramref name="baseAddress"/>.
    /// </summary>
    /// <typeparam name="TClient">The service interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="baseAddress">The base address, including any path prefix.</param>
    /// <param name="configureSettings">Configures the client's <see cref="EasyPeasySettings"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddEasyPeasyClient<TClient>(
        this IServiceCollection services,
        Uri baseAddress,
        Action<EasyPeasySettings>? configureSettings = null)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        return services
            .AddEasyPeasyClient<TClient>(configureSettings)
            .ConfigureHttpClient(client => client.BaseAddress = baseAddress);
    }
}
