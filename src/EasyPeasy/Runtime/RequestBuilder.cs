using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyPeasy.Runtime;

/// <summary>
/// Collects the arguments of one call, then builds, sends and reads the HTTP request.
/// </summary>
/// <remarks>This type supports generated code and is not intended to be used directly.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class RequestBuilder
{
    private static readonly HashSet<string> ContentHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Allow", "Content-Disposition", "Content-Encoding", "Content-Language", "Content-Length",
        "Content-Location", "Content-MD5", "Content-Range", "Content-Type", "Expires", "Last-Modified",
    };

    private readonly ClientContext context;
    private readonly RequestTemplate template;
    private Dictionary<string, string>? pathValues;
    private List<KeyValuePair<string, string>>? queryValues;
    private List<KeyValuePair<string, string>>? headerValues;
    private List<KeyValuePair<string, object>>? formValues;
    private object? body;
    private Type? bodyType;

    internal RequestBuilder(ClientContext context, RequestTemplate template)
    {
        this.context = context;
        this.template = template;
    }

    private IUrlParameterFormatter Formatter => context.Settings.UrlParameterFormatter;

    /// <summary>Binds a value to a <c>{name}</c> path variable.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The value. It must not be <see langword="null"/>.</param>
    public void AddPathParameter<T>(string name, T value)
    {
        string formatted = Formatter.Format(value)
            ?? throw new ArgumentNullException(name, $"Path parameter '{name}' cannot be null.");
        (pathValues ??= new(StringComparer.Ordinal))[name] = formatted;
    }

    /// <summary>Adds a query string value. <see langword="null"/> is skipped and sequences add one value per item.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The value.</param>
    public void AddQueryParameter<T>(string name, T value)
    {
        foreach (var item in Expand(value))
        {
            if (Formatter.Format(item) is { } formatted)
            {
                (queryValues ??= []).Add(new(name, formatted));
            }
        }
    }

    /// <summary>Adds a header value. <see langword="null"/> is skipped and sequences add one value per item.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="name">The header name.</param>
    /// <param name="value">The value.</param>
    public void AddHeader<T>(string name, T value)
    {
        foreach (var item in Expand(value))
        {
            if (Formatter.Format(item) is { } formatted)
            {
                (headerValues ??= []).Add(new(name, formatted));
            }
        }
    }

    /// <summary>Adds a form field or multipart part. <see langword="null"/> is skipped.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="name">The field name.</param>
    /// <param name="value">The value.</param>
    public void AddFormField<T>(string name, T value)
    {
        foreach (var item in Expand(value))
        {
            if (item is not null)
            {
                (formValues ??= []).Add(new(name, item));
            }
        }
    }

    /// <summary>Sets the request body.</summary>
    /// <typeparam name="T">The declared type of the body.</typeparam>
    /// <param name="value">The body.</param>
    public void SetBody<T>(T value)
    {
        body = value;
        bodyType = typeof(T) == typeof(object) && value is not null ? value.GetType() : typeof(T);
    }

    /// <summary>Sends the request and throws <see cref="ApiException"/> for an unsuccessful status code.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the response has been received.</returns>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the request and reads the response as <typeparamref name="T"/>. <see cref="HttpResponseMessage"/> is
    /// returned without checking the status code (the caller disposes it); <see cref="Stream"/> is returned unread.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The result.</returns>
    public async Task<T> SendAsync<T>(CancellationToken cancellationToken)
    {
        var response = await SendCoreAsync(cancellationToken).ConfigureAwait(false);
        if (typeof(T) == typeof(HttpResponseMessage))
        {
            return (T)(object)response;
        }

        if (typeof(T) == typeof(Stream))
        {
            try
            {
                await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
                return (T)(object)await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        using (response)
        {
            await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
            return await ReadAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Sends the request and reports the status without throwing for unsuccessful status codes.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The response.</returns>
    public async Task<ApiResponse> SendForApiResponseAsync(CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(cancellationToken).ConfigureAwait(false);
        var error = response.IsSuccessStatusCode
            ? null
            : await ApiException.CreateAsync(response, cancellationToken).ConfigureAwait(false);
        return new ApiResponse(response, error);
    }

    /// <summary>Sends the request and reads the content, without throwing for unsuccessful status codes.</summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The response.</returns>
    public async Task<ApiResponse<T>> SendForApiResponseAsync<T>(CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ApiException.CreateAsync(response, cancellationToken).ConfigureAwait(false);
            return new ApiResponse<T>(response, default, error);
        }

        var content = await ReadAsync<T>(response, cancellationToken).ConfigureAwait(false);
        return new ApiResponse<T>(response, content, null);
    }

    /// <summary>Sends the request and reads items from the response as they arrive.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="cancellationToken">Cancels the request and the enumeration.</param>
    /// <returns>The items.</returns>
    public async IAsyncEnumerable<T> StreamAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        string mediaType = ResponseMediaType(response);
        if (!context.Settings.MediaTypeHandlers.TryGetHandler(mediaType, out var handler) ||
            handler is not IStreamingMediaTypeHandler streaming)
        {
            throw new EasyPeasyException(
                $"No streaming media type handler is registered for '{mediaType}', so the response cannot be read as IAsyncEnumerable<{typeof(T).Name}>.");
        }

        await foreach (var item in streaming.DeserializeAsyncEnumerable<T>(response.Content, cancellationToken).ConfigureAwait(false))
        {
            yield return item!;
        }
    }

    internal HttpRequestMessage BuildRequest()
    {
        var request = new HttpRequestMessage(template.Method, BuildUri());
        try
        {
            request.Headers.Accept.ParseAdd(template.Accept ?? context.Settings.DefaultMediaType);
            request.Content = BuildContent();

            // Fixed headers first (interface, then method), then parameter headers. Each source replaces
            // any earlier value with the same name; repeated values from one parameter are all kept.
            foreach (var header in template.Headers)
            {
                SetHeader(request, header.Key, header.Value, replace: true);
            }

            if (headerValues is not null)
            {
                var replaced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var header in headerValues)
                {
                    SetHeader(request, header.Key, header.Value, replace: replaced.Add(header.Key));
                }
            }

            return request;
        }
        catch
        {
            request.Dispose();
            throw;
        }
    }

    private static IEnumerable<object?> Expand<T>(T value)
    {
        if (value is IEnumerable sequence and not string and not byte[])
        {
            foreach (var item in sequence)
            {
                yield return item;
            }
        }
        else
        {
            yield return value;
        }
    }

    private static void SetHeader(HttpRequestMessage request, string name, string value, bool replace)
    {
        HttpHeaders? headers = ContentHeaderNames.Contains(name) ? request.Content?.Headers : request.Headers;
        if (headers is null)
        {
            // A content header with no body to apply it to.
            return;
        }

        if (replace)
        {
            headers.Remove(name);
        }

        headers.TryAddWithoutValidation(name, value);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await ApiException.CreateAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<HttpResponseMessage> SendCoreAsync(CancellationToken cancellationToken)
    {
        using var request = BuildRequest();
        return await context.HttpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
    }

    private Uri BuildUri()
    {
        Uri baseAddress = context.BaseAddress ?? context.HttpClient.BaseAddress
            ?? throw new InvalidOperationException(
                "EasyPeasy clients need a base address. Set HttpClient.BaseAddress, or pass one to EasyPeasyClient.Create.");

        // Build the URI by hand: combining with a relative "/path" through Uri or HttpClient would
        // replace the base address path rather than append to it.
        var builder = new StringBuilder(baseAddress.GetLeftPart(UriPartial.Path));
        if (builder[^1] != '/')
        {
            builder.Append('/');
        }

        template.AppendPath(builder, pathValues);

        if (queryValues is not null)
        {
            char separator = '?';
            foreach (var pair in queryValues)
            {
                builder.Append(separator).Append(Uri.EscapeDataString(pair.Key)).Append('=').Append(Uri.EscapeDataString(pair.Value));
                separator = '&';
            }
        }

        return new Uri(builder.ToString(), UriKind.Absolute);
    }

    private HttpContent? BuildContent()
    {
        if (template.IsMultipart)
        {
            return BuildMultipartContent();
        }

        if (formValues is not null)
        {
            return new FormUrlEncodedContent(
                formValues.Select(pair => new KeyValuePair<string, string>(pair.Key, Formatter.Format(pair.Value) ?? string.Empty)));
        }

        string mediaType = template.ContentType ?? context.Settings.DefaultMediaType;
        switch (body)
        {
            case null:
                return null;
            case HttpContent content:
                return content;
            case Stream stream:
                return WithContentType(new StreamContent(stream), template.ContentType ?? MediaType.ApplicationOctetStream);
            case byte[] bytes:
                return WithContentType(new ByteArrayContent(bytes), template.ContentType ?? MediaType.ApplicationOctetStream);
        }

        if (!context.Settings.MediaTypeHandlers.TryGetHandler(mediaType, out var handler))
        {
            throw new EasyPeasyException($"No media type handler is registered for '{mediaType}', so the request body cannot be written.");
        }

        return handler.Serialize(body, bodyType!, mediaType);
    }

    private MultipartFormDataContent BuildMultipartContent()
    {
        var multipart = new MultipartFormDataContent();
        foreach (var (name, value) in formValues ?? [])
        {
            switch (value)
            {
                case FileInfo file:
                    multipart.Add(WithContentType(new StreamContent(file.OpenRead()), MimeTypes.FromFileName(file.Name)), name, file.Name);
                    break;
                case FilePart part:
                    var partContentType = part.ContentType ?? MimeTypes.FromFileName(part.FileName);
                    multipart.Add(WithContentType(new StreamContent(part.Content), partContentType), name, part.FileName);
                    break;
                case Stream stream:
                    multipart.Add(WithContentType(new StreamContent(stream), MediaType.ApplicationOctetStream), name, name);
                    break;
                case byte[] bytes:
                    multipart.Add(WithContentType(new ByteArrayContent(bytes), MediaType.ApplicationOctetStream), name, name);
                    break;
                case HttpContent content:
                    multipart.Add(content, name);
                    break;
                default:
                    multipart.Add(new StringContent(Formatter.Format(value) ?? string.Empty, Encoding.UTF8), name);
                    break;
            }
        }

        return multipart;
    }

    private static HttpContent WithContentType(HttpContent content, string mediaType)
    {
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
        return content;
    }

    private string ResponseMediaType(HttpResponseMessage response) =>
        response.Content.Headers.ContentType?.MediaType ?? template.Accept ?? context.Settings.DefaultMediaType;

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var value = await ReadContentAsync(response, typeof(T), cancellationToken).ConfigureAwait(false);
        return value is null ? default! : (T)value;
    }

    private async Task<object?> ReadContentAsync(HttpResponseMessage response, Type type, CancellationToken cancellationToken)
    {
        if (type == typeof(byte[]))
        {
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        if (response.StatusCode == HttpStatusCode.NoContent ||
            template.Method == HttpMethod.Head ||
            response.Content.Headers.ContentLength == 0)
        {
            return null;
        }

        string mediaType = ResponseMediaType(response);
        if (context.Settings.MediaTypeHandlers.TryGetHandler(mediaType, out var handler))
        {
            return await handler.DeserializeAsync(response.Content, type, cancellationToken).ConfigureAwait(false);
        }

        if (type == typeof(string))
        {
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        throw new EasyPeasyException(
            $"No media type handler is registered for '{mediaType}', so the response cannot be read as '{type}'.");
    }
}
