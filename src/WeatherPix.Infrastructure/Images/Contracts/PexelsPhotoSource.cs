using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Images.Contracts;

public record PexelsPhotoSource
{
    [JsonPropertyName("large")]
    public required string Large { get; init; }
}