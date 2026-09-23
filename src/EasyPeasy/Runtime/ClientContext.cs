using System.ComponentModel;

namespace EasyPeasy.Runtime;

/// <summary>
/// The <see cref="System.Net.Http.HttpClient"/> and settings shared by every method of one generated client.
/// </summary>
/// <remarks>This type supports generated code and is not intended to be used directly.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ClientContext
{
    /// <summary>Initializes a new instance of the <see cref="ClientContext"/> class.</summary>
    /// <param name="httpClient">The HTTP client that sends requests.</param>
    /// <param name="settings">The client settings.</param>
    /// <param name="baseAddress">The base address, or <see langword="null"/> to use <see cref="HttpClient.BaseAddress"/>.</param>
    public ClientContext(HttpClient httpClient, EasyPeasySettings settings, Uri? baseAddress = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(settings);

        HttpClient = httpClient;
        Settings = settings;
        BaseAddress = baseAddress;
    }

    /// <summary>Gets the HTTP client that sends requests.</summary>
    public HttpClient HttpClient { get; }

    /// <summary>Gets the client settings.</summary>
    public EasyPeasySettings Settings { get; }

    /// <summary>Gets the base address, or <see langword="null"/> to use <see cref="HttpClient.BaseAddress"/>.</summary>
    public Uri? BaseAddress { get; }

    /// <summary>Starts building a request from a template.</summary>
    /// <param name="template">The method's request template.</param>
    /// <returns>The request builder.</returns>
    public RequestBuilder CreateRequest(RequestTemplate template) => new(this, template);
}
