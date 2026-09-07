using System.Net.Http.Json;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.WeatherStation;
using WeatherPix.Infrastructure.Weather.Contracts;

namespace WeatherPix.Infrastructure.Weather;

public class BuienradarWeatherStationProvider(
    HttpClient httpClient)
    : IWeatherStationProvider
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IReadOnlyCollection<WeatherStation>> GetStationsAsync(
        CancellationToken ct)
    {
        var response = await _httpClient
            .GetFromJsonAsync<BuienradarResponse>("", ct);

        return response is null
            ? throw new InvalidOperationException(
                "Buienradar returned an empty response.")
            : [.. response.Actual.StationMeasurements.Select(Map)];
    }

    private static WeatherStation Map(
        BuienradarStationResponse station)
    {
        return new WeatherStation(
            station.StationId,
            station.StationName ?? "Unknown",
            station.Region ?? "Unknown",
            station.Latitude,
            station.Longitude,
            station.Temperature ?? 0,
            station.Humidity ?? 0,
            station.WindDirection ?? "Unknown",
            station.WindSpeed ?? 0,
            station.WindGusts ?? 0,
            station.AirPressure ?? 0,
            station.Visibility ?? 0,
            station.Precipitation ?? 0,
            station.Timestamp);
    }
}