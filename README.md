EasyPeasy
==========

EasyPeasy aims to get rid of the boiler plate coding required when talking
to REST APIs. 

Simply define an interface to represent the REST service and annotate with
path, verb and parameter attributes to influence how the method are called.

The library supports both synchronous and asynchronous method calls, just
define a method with a return type of either Task or Task<T> and EasyPeasy will
do the rest for you!

Interfaces and methods are annotated using [JAX-RS](http://en.wikipedia.org/wiki/Java_API_for_RESTful_Web_Services) style attributes

### Example

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

        [PUT, Path("/{name}"), Consumes(MediaType.ApplicationUrlEncoded)]
        void UpdateCustomer([PathParam("name")] string name, [FormParam("address")] string address);
    }

An implementation of this interface can then be generated for you using those attributes. Simply create a factory and call Create:

    ICustomerService client = new EasyPeasyFactory().Create<ICustomerService>(new Uri("http://server.com"));
    
    Customer customer = client.GetCustomer("My Customer");

Being a fan of MEF, the types are all Exportable.  Below is an example of grabbing an IEasyPeasyFactory from a container:

    AssemblyCatalog catalog = new AssemblyCatalog(typeof(IEasyPeasyFactory).Assembly);
    CompositionContainer container = new CompositionContainer(catalog);

    IEasyPeasyFactory factory = container.GetExportedValue<IEasyPeasyFactory>();

    ICustomerService client = factory.Create<ICustomerService>(new Uri("http://server.com"));
    Customer customer = client.GetCustomer("My Customer");

