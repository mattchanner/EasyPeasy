using EasyPeasy.Attributes;

namespace EasyPeasy.Example;

// The interface-level Path is prepended to every method path, and appended to the client's base address.
//
// Produces picks the serializer for request bodies and sets the Content-Type header.
// Consumes sets the Accept header, and picks the deserializer when a response has no Content-Type.
// Both default to application/json, so they are shown here only for illustration.
[Path("/api/contact"), Consumes(MediaType.ApplicationJson), Produces(MediaType.ApplicationJson)]
public interface IContactService
{
    // The path can be given on the HTTP method attribute. {name} is filled from the [PathParam]
    // parameter and escaped, so a value cannot change the path.
    [GET("/{name}")]
    Task<Contact> GetContactAsync([PathParam] string name, CancellationToken cancellationToken = default);

    // Returning ApiResponse<T> reports 4xx/5xx through the result instead of throwing ApiException.
    [GET("/{name}")]
    Task<ApiResponse<Contact>> TryGetContactAsync([PathParam] string name, CancellationToken cancellationToken = default);

    [GET]
    Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default);

    // IAsyncEnumerable<T> yields items while the JSON array is still arriving.
    [GET]
    IAsyncEnumerable<Contact> StreamContactsAsync(CancellationToken cancellationToken = default);

    // A parameter without a binding attribute is the request body.
    [POST]
    Task CreateContactAsync(Contact contact, CancellationToken cancellationToken = default);

    [PUT("/{name}")]
    Task UpdateContactAsync([PathParam] string name, Contact contact, CancellationToken cancellationToken = default);

    // [FormParam] parameters are sent as application/x-www-form-urlencoded: address=value
    [PUT("/{name}/address")]
    Task UpdateAddressAsync([PathParam] string name, [FormParam] string address, CancellationToken cancellationToken = default);

    [DELETE("/{name}")]
    Task DeleteContactAsync([PathParam] string name, CancellationToken cancellationToken = default);
}
