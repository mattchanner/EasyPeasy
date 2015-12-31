using System.Threading.Tasks;

using EasyPeasy.Attributes;

namespace EasyPeasy.Tests.TestTypes.Reflected
{
    /// <summary>
    /// Annotated interface to test reflection helpers determine the correct meta data
    /// </summary>
    [Consumes(MediaType.ApplicationJson),
     Produces(MediaType.ApplicationXml),
     Path("/services/{apiVersion}")]
    public interface ITestService1
    {
        [Path("/data")]
        Task<CollectionDto> GetData([QueryParam("q")] string query);

        /// <summary>
        /// Overrides Consumes and Produces
        /// </summary>
        /// <param name="body">The body of the request</param>
        /// <returns></returns>
        [POST, Path("/update"), Consumes(MediaType.TextXml), Produces(MediaType.ApplicationOctetStream)]
        Task Update(SimpleDto body);
    }
}
