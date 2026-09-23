using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;

namespace EasyPeasy;

/// <summary>
/// The result of a call whose method returns <c>Task&lt;ApiResponse&gt;</c>. Unsuccessful status codes are
/// reported through <see cref="Error"/> instead of being thrown.
/// </summary>
public class ApiResponse
{
    /// <summary>Initializes a new instance of the <see cref="ApiResponse"/> class.</summary>
    /// <param name="response">The HTTP response. Its headers are kept; the response itself is not.</param>
    /// <param name="error">The error, when the status code was unsuccessful.</param>
    public ApiResponse(HttpResponseMessage response, ApiException? error)
    {
        ArgumentNullException.ThrowIfNull(response);
        StatusCode = response.StatusCode;
        ReasonPhrase = response.ReasonPhrase;
        Version = response.Version;
        Headers = response.Headers;
        ContentHeaders = response.Content.Headers;
        Error = error;
    }

    /// <summary>Gets the response status code.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Gets the response reason phrase.</summary>
    public string? ReasonPhrase { get; }

    /// <summary>Gets the HTTP version of the response.</summary>
    public Version Version { get; }

    /// <summary>Gets the response headers.</summary>
    public HttpResponseHeaders Headers { get; }

    /// <summary>Gets the response content headers.</summary>
    public HttpContentHeaders ContentHeaders { get; }

    /// <summary>Gets the error, when the status code was unsuccessful.</summary>
    public ApiException? Error { get; }

    /// <summary>Gets a value indicating whether the status code was in the 2xx range.</summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccessStatusCode => Error is null;

    /// <summary>Throws <see cref="Error"/> if the status code was unsuccessful.</summary>
    /// <returns>This response.</returns>
    public ApiResponse EnsureSuccessStatusCode()
    {
        if (Error is not null)
        {
            throw Error;
        }

        return this;
    }
}

/// <summary>
/// The result of a call whose method returns <c>Task&lt;ApiResponse&lt;T&gt;&gt;</c>: the deserialized
/// <see cref="Content"/> plus the status and headers.
/// </summary>
/// <typeparam name="T">The content type.</typeparam>
public sealed class ApiResponse<T> : ApiResponse
{
    /// <summary>Initializes a new instance of the <see cref="ApiResponse{T}"/> class.</summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="content">The deserialized content.</param>
    /// <param name="error">The error, when the status code was unsuccessful.</param>
    public ApiResponse(HttpResponseMessage response, T? content, ApiException? error)
        : base(response, error)
    {
        Content = content;
    }

    /// <summary>Gets the deserialized content, or <see langword="default"/> when the call failed or had no body.</summary>
    public T? Content { get; }

    /// <summary>Throws <see cref="ApiResponse.Error"/> if the status code was unsuccessful.</summary>
    /// <returns>This response.</returns>
    public new ApiResponse<T> EnsureSuccessStatusCode()
    {
        base.EnsureSuccessStatusCode();
        return this;
    }
}
