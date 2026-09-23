using System.ComponentModel;
using System.Text;

namespace EasyPeasy.Runtime;

/// <summary>
/// The fixed, per-method part of a request: verb, path template, media types and static headers.
/// Generated clients create one of these per interface method.
/// </summary>
/// <remarks>This type supports generated code and is not intended to be used directly.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class RequestTemplate
{
    private readonly Segment[] segments;

    /// <summary>Initializes a new instance of the <see cref="RequestTemplate"/> class.</summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path relative to the base address, with <c>{name}</c> variables.</param>
    /// <param name="accept">The media type to accept, or <see langword="null"/> for the default.</param>
    /// <param name="contentType">The media type of the body, or <see langword="null"/> for the default.</param>
    /// <param name="headers">Fixed headers to send, in order. Later entries replace earlier ones with the same name.</param>
    /// <param name="isMultipart">Whether form fields are sent as <c>multipart/form-data</c>.</param>
    public RequestTemplate(
        HttpMethod method,
        string path,
        string? accept = null,
        string? contentType = null,
        IReadOnlyList<KeyValuePair<string, string>>? headers = null,
        bool isMultipart = false)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(path);

        Method = method;
        Path = path;
        Accept = accept;
        ContentType = contentType;
        Headers = headers ?? [];
        IsMultipart = isMultipart;
        segments = Parse(path.TrimStart('/'));
    }

    /// <summary>Gets the HTTP method.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets the path template.</summary>
    public string Path { get; }

    /// <summary>Gets the media type to accept, or <see langword="null"/> for the default.</summary>
    public string? Accept { get; }

    /// <summary>Gets the media type of the body, or <see langword="null"/> for the default.</summary>
    public string? ContentType { get; }

    /// <summary>Gets the fixed headers.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Headers { get; }

    /// <summary>Gets a value indicating whether form fields are sent as <c>multipart/form-data</c>.</summary>
    public bool IsMultipart { get; }

    internal void AppendPath(StringBuilder builder, Dictionary<string, string>? values)
    {
        foreach (var segment in segments)
        {
            if (!segment.IsVariable)
            {
                builder.Append(segment.Text);
                continue;
            }

            if (values is null || !values.TryGetValue(segment.Text, out var value))
            {
                throw new EasyPeasyException($"No value was supplied for path variable '{{{segment.Text}}}' in '{Path}'.");
            }

            // EscapeDataString escapes '/', '?' and '#', so a value stays inside its segment. A whole
            // segment of "." or ".." cannot be escaped: Uri decodes %2E and removes dot segments anyway.
            if (value is "." or "..")
            {
                throw new ArgumentException(
                    $"Path variable '{segment.Text}' cannot be '{value}', which would change the request path.", segment.Text);
            }

            builder.Append(Uri.EscapeDataString(value));
        }
    }

    private static Segment[] Parse(string path)
    {
        var result = new List<Segment>();
        int position = 0;
        while (position < path.Length)
        {
            int open = path.IndexOf('{', position);
            if (open < 0)
            {
                result.Add(new Segment(path[position..], false));
                break;
            }

            int close = path.IndexOf('}', open + 1);
            if (close < 0)
            {
                throw new ArgumentException($"Path '{path}' has an unclosed '{{'.", nameof(path));
            }

            if (open > position)
            {
                result.Add(new Segment(path[position..open], false));
            }

            result.Add(new Segment(path[(open + 1)..close], true));
            position = close + 1;
        }

        return [.. result];
    }

    private readonly record struct Segment(string Text, bool IsVariable);
}
