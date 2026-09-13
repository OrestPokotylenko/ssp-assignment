using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Weather.Contracts;

public sealed class BuienradarResponse
{
    [JsonPropertyName("actual")]
    public required BuienradarActual Actual { get; init; }
}

public sealed class BuienradarActual
{
    [JsonPropertyName("stationmeasurements")]
    public required List<BuienradarStationResponse> StationMeasurements { get; init; }
}