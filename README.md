EasyPeasy
==========

EasyPeasy is a library for .Net clients, using jax-rs style annotations on interfaces to automatically generate implementations
that can be used to communicate with a RESTful service.

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

The proxy type can then be created simply by calling:

    IEasyPeasyFactory factory = new EasyPeasyFactory(new DefaultMediaTypeRegistry());
    ICustomerService client = factory.Create<ICustomerService>(new Uri("http://server.com"));
    Customer customer = client.GetCustomer("My Customer");

Or, using MEF:

    AssemblyCatalog catalog = new AssemblyCatalog(typeof(IEasyPeasyFactory).Assembly);
    CompositionContainer container = new CompositionContainer(catalog);

    IEasyPeasyFactory factory = container.GetExportedValue<IEasyPeasyFactory>();

    ICustomerService client = factory.Create<ICustomerService>(new Uri("http://server.com"));
    Customer customer = client.GetCustomer("My Customer");

### The following attributes are supported:

* [Path] - Specifies the relative path for a resource class or method.
* [GET], [PUT], [POST], [DELETE] specify the HTTP request type of a resource.
* [Produces] specifies the response MIME media types.
* [Consumes] specifies the accepted request media types.

In addition, further attributes to method parameters to pull information out of the request are available.  All the *Param attributes take a key of some form which is used to look up the value required.

* [PathParam] binds the parameter to a path segment
* [QueryParam] binds the parameter to the value of an HTTP query parameter
* [HeaderParam] binds the parameter to an HTTP header value.
* [FormParam] binds the parameter to a form value

### Not yet implemented:

* MatrixParam
* CookieParam
* DefaultValue

### Media Type encoding \ decoding

The following media types are supported out of the box:

* application/json (using Newtonsoft.Json)
* application/xml  (uses XmlSerializer)
* text/xml
* text/plain
* text/html
* image/png
* image/gif
* image/jpeg
* image/bmp
* image/tiff (to the extent that TIFF files are supported by the .NET framework)