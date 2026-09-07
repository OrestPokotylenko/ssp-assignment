using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Weather.Contracts;

public record BuienradarStationResponse
{
    [JsonPropertyName("stationid")]
    public int StationId { get; init; }

    [JsonPropertyName("stationname")]
    public string? StationName { get; init; }

    [JsonPropertyName("regio")]
    public string? Region { get; init; }

    [JsonPropertyName("lat")]
    public double Latitude { get; init; }

    [JsonPropertyName("lon")]
    public double Longitude { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("humidity")]
    public double? Humidity { get; init; }

    [JsonPropertyName("winddirection")]
    public string? WindDirection { get; init; }

    [JsonPropertyName("windspeed")]
    public double? WindSpeed { get; init; }

    [JsonPropertyName("windgusts")]
    public double? WindGusts { get; init; }

    [JsonPropertyName("airpressure")]
    public double? AirPressure { get; init; }

    [JsonPropertyName("visibility")]
    public double? Visibility { get; init; }

    [JsonPropertyName("precipitation")]
    public double? Precipitation { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }
}