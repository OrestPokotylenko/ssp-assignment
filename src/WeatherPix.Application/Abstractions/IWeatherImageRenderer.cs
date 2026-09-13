using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Abstractions;

public interface IWeatherImageRenderer
{
    Task<Stream> RenderAsync(
        Stream sourceImage,
        WeatherStation data,
        CancellationToken ct);
}