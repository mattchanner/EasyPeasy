using EasyPeasy;
using EasyPeasy.Example;
using Microsoft.Extensions.DependencyInjection;

// Start EasyPeasy.Example.Server first; it listens on http://localhost:9000.
var baseAddress = new Uri("http://localhost:9000");

// Register the client with IHttpClientFactory. The builder it returns is a normal IHttpClientBuilder,
// so handlers, resilience (retries, timeouts, circuit breaker) and telemetry are added in the usual way.
var services = new ServiceCollection();
services.AddTransient<LoggingHandler>();
services
    .AddEasyPeasyClient<IContactService>(baseAddress)
    .AddHttpMessageHandler<LoggingHandler>()
    .AddStandardResilienceHandler();

await using var provider = services.BuildServiceProvider();
var contacts = provider.GetRequiredService<IContactService>();

// Without dependency injection, this does the same using a shared, pooled HttpClient:
//   var contacts = EasyPeasyClient.Create<IContactService>(baseAddress);

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var ct = cancellation.Token;

// GET http://localhost:9000/api/contact
Console.WriteLine("All contacts:");
foreach (var contact in await contacts.GetContactsAsync(ct))
{
    Console.WriteLine($"  {contact.Name}: {contact.Address}");
}

// GET http://localhost:9000/api/contact/Contact1
var first = await contacts.GetContactAsync("Contact1", ct);
Console.WriteLine($"Fetched {first.Name}: {first.Address}");

// PUT http://localhost:9000/api/contact/Contact1 with a JSON body
first.Address = "Changed address";
await contacts.UpdateContactAsync(first.Name, first, ct);

// PUT http://localhost:9000/api/contact/Contact3/address with a form body: address=Updated_using_form_param
await contacts.UpdateAddressAsync("Contact3", "Updated_using_form_param", ct);

// POST http://localhost:9000/api/contact
await contacts.CreateContactAsync(new Contact { Name = "Contact4", Address = "Address4" }, ct);

// DELETE http://localhost:9000/api/contact/Contact2
await contacts.DeleteContactAsync("Contact2", ct);

// Stream the updated list: items are yielded while the response is still being read.
Console.WriteLine("Contacts after the updates:");
await foreach (var contact in contacts.StreamContactsAsync(ct))
{
    Console.WriteLine($"  {contact.Name}: {contact.Address}");
}

// Errors: ApiResponse<T> reports them without throwing...
var missing = await contacts.TryGetContactAsync("Contact2", ct);
Console.WriteLine($"Contact2 after delete: {(int)missing.StatusCode} {missing.StatusCode}");

// ...and other methods throw ApiException, which keeps the status code and response body.
try
{
    await contacts.GetContactAsync("Nobody", ct);
}
catch (ApiException ex)
{
    Console.WriteLine($"Expected failure: {(int)ex.StatusCode}, body: {ex.Content}");
}

Console.WriteLine("Done!");
