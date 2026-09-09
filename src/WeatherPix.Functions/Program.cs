using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherPix.Application.DependencyInjection;
using WeatherPix.Functions.DependencyInjection;
using WeatherPix.Infrastructure.DependencyInjection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.Converters.Add(new JsonStringEnumConverter(
        JsonNamingPolicy.CamelCase));
});

var cfg = builder.Configuration;

builder.Services
    .AddApplication(cfg)
    .AddInfrastructure(cfg, builder.Environment)
    .AddAuth(cfg);

builder.Build().Run();