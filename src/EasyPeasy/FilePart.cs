namespace EasyPeasy;

/// <summary>
/// A file to upload as part of a <c>multipart/form-data</c> request.
/// </summary>
/// <param name="Content">The file content. It is disposed after the request is sent.</param>
/// <param name="FileName">The file name sent to the server.</param>
/// <param name="ContentType">The media type of the file. Defaults to one inferred from the file extension.</param>
public sealed record FilePart(Stream Content, string FileName, string? ContentType = null)
{
    /// <summary>Creates a <see cref="FilePart"/> that reads a file from disk.</summary>
    /// <param name="path">The file path.</param>
    /// <param name="contentType">The media type, or <see langword="null"/> to infer it from the extension.</param>
    /// <returns>The file part.</returns>
    public static FilePart FromFile(string path, string? contentType = null) =>
        new(File.OpenRead(path), System.IO.Path.GetFileName(path), contentType);
}
