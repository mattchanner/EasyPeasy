EasyPeasy
==========

EasyPeasy is a library for .Net clients, using jax-rs style annotations on interfaces to automatically generate implementations
that can be used to communicate with a RESTful service.

`
[Path("/services/customers"), Consumes("application/json"), Produces("application/json")] 
public interface ICustomerService
{
     // Built in support for async requests
    [GET, Path("/{name}")]
    Task<Customer> GetCustomerAsync([PathParam("name")] string name);
 
    // Synchronous equivalent
    [GET, Path("/{name}")]
    Customer GetCustomer([PathParam("name")] string name);

    [DELETE, Path("/{name}")]
    void DeleteCustomer([PathParam("name")] string name, [QueryParam("q")] bool q);

    [DELETE, Path("/{name}")]
    Task DeleteCustomerAsync([PathParam("name")] string name, [QueryParam("q")] bool q);
}
`

The proxy type can then be created simply by calling:

`
ICustomerService client = ServiceProxy.CreateProxy<ICustomerService>(new Uri("http://server.com"));
Customer customer = client.GetCustomer("My Customer");
`

