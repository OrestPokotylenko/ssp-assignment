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
            station.StationName,
            station.Latitude,
            station.Longitude,
            station.Region,
            station.Timestamp,
            station.WindDirection,
            station.WeatherDescription,
            station.AirPressure,
            station.Temperature,
            station.FeelTemperature,
            station.Visibility,
            station.WindGusts,
            station.WindSpeed,
            station.Humidity);
    }
}