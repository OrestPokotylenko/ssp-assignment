using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Weather.Contracts;

public record BuienradarStationResponse
{
    [JsonPropertyName("stationid")]
    public int StationId { get; init; }

    [JsonPropertyName("stationname")]
    public string? StationName { get; init; }

    [JsonPropertyName("lat")]
    public double Latitude { get; init; }

    [JsonPropertyName("lon")]
    public double Longitude { get; init; }

    [JsonPropertyName("regio")]
    public string? Region { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }

    [JsonPropertyName("weatherdescription")]
    public string? WeatherDescription { get; init; }

    [JsonPropertyName("winddirection")]
    public string? WindDirection { get; init; }

    [JsonPropertyName("airpressure")]
    public double? AirPressure { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("feeltemperature")]
    public double? FeelTemperature { get; init; }

    [JsonPropertyName("visibility")]
    public double? Visibility { get; init; }

    [JsonPropertyName("windgusts")]
    public double? WindGusts { get; init; }

    [JsonPropertyName("windspeed")]
    public double? WindSpeed { get; init; }

    [JsonPropertyName("humidity")]
    public double? Humidity { get; init; }
}