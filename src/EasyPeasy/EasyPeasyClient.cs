using EasyPeasy.Runtime;

namespace EasyPeasy;

/// <summary>
/// Creates clients for service interfaces. The implementations are generated at compile time.
/// </summary>
/// <remarks>
/// In applications that use dependency injection, prefer <c>services.AddEasyPeasyClient&lt;T&gt;()</c> from the
/// <c>EasyPeasy.Extensions.DependencyInjection</c> package, which uses <c>IHttpClientFactory</c>.
/// </remarks>
public static class EasyPeasyClient
{
    // One pooled handler for clients created from a base address. PooledConnectionLifetime makes it pick up
    // DNS changes, which is what a long-lived HttpClient otherwise misses.
    private static readonly Lazy<HttpClient> SharedHttpClient = new(() =>
        new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) }));

    /// <summary>Creates a client that sends requests through <paramref name="httpClient"/>.</summary>
    /// <typeparam name="T">The service interface.</typeparam>
    /// <param name="httpClient">
    /// The HTTP client. Its <see cref="HttpClient.BaseAddress"/> is used, including any path it contains.
    /// The client is not disposed by EasyPeasy.
    /// </param>
    /// <param name="settings">The settings, or <see langword="null"/> for the defaults.</param>
    /// <returns>The client.</returns>
    public static T Create<T>(HttpClient httpClient, EasyPeasySettings? settings = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        return ProxyRegistry.Create<T>(new ClientContext(httpClient, settings ?? new EasyPeasySettings()));
    }

    /// <summary>Creates a client for a base address, using a shared, pooled <see cref="HttpClient"/>.</summary>
    /// <typeparam name="T">The service interface.</typeparam>
    /// <param name="baseAddress">The base address, including any path prefix.</param>
    /// <param name="settings">The settings, or <see langword="null"/> for the defaults.</param>
    /// <returns>The client.</returns>
    public static T Create<T>(Uri baseAddress, EasyPeasySettings? settings = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        if (!baseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("The base address must be an absolute URI.", nameof(baseAddress));
        }

        return ProxyRegistry.Create<T>(new ClientContext(SharedHttpClient.Value, settings ?? new EasyPeasySettings(), baseAddress));
    }

    /// <summary>Creates a client for a base address, using a shared, pooled <see cref="HttpClient"/>.</summary>
    /// <typeparam name="T">The service interface.</typeparam>
    /// <param name="baseAddress">The base address, including any path prefix.</param>
    /// <param name="settings">The settings, or <see langword="null"/> for the defaults.</param>
    /// <returns>The client.</returns>
    public static T Create<T>(string baseAddress, EasyPeasySettings? settings = null)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseAddress);
        return Create<T>(new Uri(baseAddress, UriKind.Absolute), settings);
    }
}
