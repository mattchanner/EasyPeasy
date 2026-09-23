const string BaseAddress = "http://localhost:9000";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

Console.WriteLine("Listening on {0}", BaseAddress);
await app.RunAsync(BaseAddress);
