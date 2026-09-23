using System.Net;
using System.Net.Http.Headers;

namespace EasyPeasy;

/// <summary>
/// Thrown when a service returns a status code outside the 2xx range. Unlike
/// <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>, it keeps the response body and headers.
/// </summary>
/// <remarks>
/// It derives from <see cref="HttpRequestException"/>, so existing <c>catch (HttpRequestException)</c> blocks still apply.
/// Network failures and timeouts are not wrapped: they surface as the <see cref="HttpRequestException"/> or
/// <see cref="TaskCanceledException"/> thrown by <see cref="HttpClient"/>.
/// </remarks>
public class ApiException : HttpRequestException
{
    /// <summary>Initializes a new instance of the <see cref="ApiException"/> class.</summary>
    /// <param name="method">The request method.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="reasonPhrase">The response reason phrase.</param>
    /// <param name="headers">The response headers.</param>
    /// <param name="contentHeaders">The response content headers.</param>
    /// <param name="content">The response body.</param>
    public ApiException(
        HttpMethod method,
        Uri? requestUri,
        HttpStatusCode statusCode,
        string? reasonPhrase,
        HttpResponseHeaders headers,
        HttpContentHeaders? contentHeaders,
        string? content)
        : base(CreateMessage(method, requestUri, statusCode, reasonPhrase), null, statusCode)
    {
        Method = method;
        RequestUri = requestUri;
        ReasonPhrase = reasonPhrase;
        Headers = headers;
        ContentHeaders = contentHeaders;
        Content = content;
    }

    /// <summary>Gets the request method.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the request URI.</summary>
    public Uri? RequestUri { get; }

    /// <summary>Gets the response status code.</summary>
    public new HttpStatusCode StatusCode => base.StatusCode!.Value;

    /// <summary>Gets the response reason phrase.</summary>
    public string? ReasonPhrase { get; }

    /// <summary>Gets the response headers.</summary>
    public HttpResponseHeaders Headers { get; }

    /// <summary>Gets the response content headers, if the response had content.</summary>
    public HttpContentHeaders? ContentHeaders { get; }

    /// <summary>Gets the response body as text, or <see langword="null"/> if there was none.</summary>
    public string? Content { get; }

    /// <summary>Gets a value indicating whether the response had a body.</summary>
    public bool HasContent => !string.IsNullOrEmpty(Content);

    /// <summary>Creates an <see cref="ApiException"/> from a failed response, reading its body.</summary>
    /// <param name="response">The response.</param>
    /// <param name="cancellationToken">Cancels reading the body.</param>
    /// <returns>The exception.</returns>
    public static async Task<ApiException> CreateAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        string? content = null;
        try
        {
            content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            // The status code is what matters; a body that cannot be read is left out.
        }

        return new ApiException(
            response.RequestMessage?.Method ?? HttpMethod.Get,
            response.RequestMessage?.RequestUri,
            response.StatusCode,
            response.ReasonPhrase,
            response.Headers,
            response.Content.Headers,
            content);
    }

    private static string CreateMessage(HttpMethod method, Uri? requestUri, HttpStatusCode statusCode, string? reasonPhrase) =>
        $"Response status code does not indicate success: {(int)statusCode} ({reasonPhrase ?? statusCode.ToString()}). Request: {method} {requestUri}";
}
