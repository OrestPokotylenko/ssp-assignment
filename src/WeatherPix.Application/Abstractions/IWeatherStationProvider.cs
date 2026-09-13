using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Abstractions;

public interface IWeatherStationProvider
{
    Task<IReadOnlyCollection<WeatherStation>> GetStationsAsync(
        CancellationToken ct);
}