using WeatherPix.Application.Common.Results;
using WeatherPix.Application.Messaging;

namespace WeatherPix.Application.Images.Handler;

public interface IGenerateWeatherImageHandler
{
    Task<Result> HandleAsync(
        GenerateImageMessage message,
        CancellationToken ct);
}