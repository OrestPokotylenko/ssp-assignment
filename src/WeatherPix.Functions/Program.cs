using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using WeatherPix.Application.DependencyInjection;
using WeatherPix.Infrastructure.DependencyInjection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Build().Run();