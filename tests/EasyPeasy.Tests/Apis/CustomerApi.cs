using EasyPeasy.Attributes;

namespace EasyPeasy.Tests.Apis;

public sealed record Customer(int Id, string Name);

[Path("/customers"), Consumes(MediaType.ApplicationJson), Produces(MediaType.ApplicationJson)]
public interface ICustomerApi
{
    [GET("/{id}")]
    Task<Customer> GetAsync([PathParam] int id, CancellationToken cancellationToken = default);

    [GET("/{id}")]
    Task<Customer?> FindAsync([PathParam] int id);

    [GET]
    Task<List<Customer>> SearchAsync(
        [QueryParam("name")] string? name = null,
        [QueryParam] int[]? ids = null,
        [QueryParam] bool? active = null,
        [QueryParam] DateTimeOffset? since = null);

    [POST]
    Task<Customer> CreateAsync(Customer customer);

    [PUT("/{id}")]
    Task UpdateAsync([PathParam] int id, [Body] Customer customer);

    [PATCH("/{id}")]
    Task<Customer> RenameAsync([PathParam] int id, [QueryParam] string name);

    [DELETE("/{id}")]
    Task DeleteAsync([PathParam] int id);

    [HEAD("/{id}")]
    Task<HttpResponseMessage> HeadAsync([PathParam] int id);

    [OPTIONS]
    Task<HttpResponseMessage> OptionsAsync();

    [GET("/{id}")]
    Task<ApiResponse<Customer>> TryGetAsync([PathParam] int id);

    [DELETE("/{id}")]
    Task<ApiResponse> TryDeleteAsync([PathParam] int id);

    [GET("/{id}")]
    ValueTask<Customer> GetValueTaskAsync([PathParam] int id);

    [DELETE("/{id}")]
    ValueTask DeleteValueTaskAsync([PathParam] int id);

    [GET("/stream")]
    IAsyncEnumerable<Customer> StreamAsync(CancellationToken cancellationToken = default);

    [GET("/count")]
    Task<int> CountAsync();

    [GET("/name")]
    Task<string> NameAsync();

    [GET("/{id}/photo")]
    Task<byte[]> PhotoAsync([PathParam] int id);

    [GET("/{id}/photo")]
    Task<Stream> PhotoStreamAsync([PathParam] int id);

    [PUT("/{id}/photo"), Produces(MediaType.ImagePNG)]
    Task UploadPhotoAsync([PathParam] int id, byte[] photo);

    [PUT("/{id}/raw"), Produces(MediaType.ApplicationOctetStream)]
    Task UploadStreamAsync([PathParam] int id, Stream content);
}
