using EasyPeasy.Attributes;

namespace EasyPeasy.Tests.Apis
{
    [Header("X-Api-Version", "1")]
    public interface IHeaderApi
    {
        [GET]
        Task InterfaceHeaderAsync();

        [GET, Header("X-Api-Version", "2"), Header("X-Trace", "on")]
        Task MethodHeadersAsync();

        [GET]
        Task ParameterHeadersAsync([HeaderParam("X-Api-Version")] string version, [HeaderParam("Authorization")] string? authorization);

        [GET]
        Task RepeatedHeaderAsync([HeaderParam("X-Tag")] string[] tags);

        [POST]
        Task ContentTypeHeaderAsync([HeaderParam("Content-Type")] string contentType, [Body] string body);

        [GET]
        Task AcceptHeaderAsync([HeaderParam("Accept")] string accept);
    }

    public interface IFormApi
    {
        [POST("/form")]
        Task UrlEncodedAsync([FormParam("first_name")] string firstName, [FormParam] string[] tags, [FormParam] string? skipped = null);

        [POST("/multipart"), Multipart]
        Task MultipartFieldsAsync([FormParam] string title, [FormParam] int count);

        [POST("/files")]
        Task UploadFileAsync(FileInfo file, [FormParam("description")] string description);

        [POST("/parts")]
        Task UploadPartAsync([FormParam] FilePart document);
    }

    public interface IDefaultsApi
    {
        // No Path, Consumes or Produces anywhere: defaults apply.
        [GET("items")]
        Task<Customer> GetAsync();

        [POST("items")]
        Task PostAsync(Customer customer);

        [GET]
        Task RootAsync();

        [GET("items/{id}")]
        Task EscapedAsync([PathParam] string id);

        [GET("items")]
        Task QueryAsync([QueryParam] string? q, [QueryParam] DateOnly? on, [QueryParam] DayOfWeek? day, [QueryParam] double? ratio);
    }

    [Consumes(MediaType.TextPlain)]
    public interface ITextApi
    {
        [GET("/text")]
        Task<string> GetTextAsync();

        [GET("/number")]
        Task<int> GetNumberAsync();

        [GET("/number")]
        Task<int?> GetOptionalNumberAsync();

        [POST("/text"), Produces(MediaType.TextPlain)]
        Task SendTextAsync(string text);
    }

    public interface INotAnEasyPeasyApi
    {
        Task DoSomethingAsync();
    }
}

// Same interface name in two namespaces (regression for bug 2).
namespace EasyPeasy.Tests.Apis.First
{
    [Path("/first")]
    public interface IDuplicateName
    {
        [GET]
        Task<string> WhoAmIAsync();
    }
}

namespace EasyPeasy.Tests.Apis.Second
{
    [Path("/second")]
    public interface IDuplicateName
    {
        [GET]
        Task<string> WhoAmIAsync();
    }
}

namespace EasyPeasy.Tests.Apis
{
    // Inherited interfaces (regression for bug 5).
    [Path("/base")]
    public interface IBaseApi
    {
        [GET("/ping")]
        Task<string> PingAsync();
    }

    [Path("/derived")]
    public interface IDerivedApi : IBaseApi
    {
        [GET("/pong")]
        Task<string> PongAsync();
    }

    public interface IEmptyDerivedApi : IBaseApi
    {
    }
}
