using Microsoft.AspNetCore.Builder;
using weather_backend.Extensions;
using weather_backend.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddStructuredLogging(builder.Environment);
builder.Services.AddWeatherApi(builder.Configuration);

var app = builder.Build();

app.UseWeatherApi();

await app.RunWithTasksAsync();

/// <summary>
/// Declared so the integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>; top-level statements
/// otherwise generate an internal Program class.
/// </summary>
public partial class Program;
