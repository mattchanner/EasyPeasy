using System.Diagnostics;

namespace EasyPeasy.Example;

// Replaces the IRequestInterceptor from EasyPeasy 2.x. A DelegatingHandler sees every request,
// response and network failure, and works with any HttpClient, not just EasyPeasy clients.
public sealed class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine($"  [{request.Method} {request.RequestUri}] {(int)response.StatusCode} in {stopwatch.ElapsedMilliseconds} ms");
            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"  [{request.Method} {request.RequestUri}] failed: {ex.Message}");
            throw;
        }
    }
}
