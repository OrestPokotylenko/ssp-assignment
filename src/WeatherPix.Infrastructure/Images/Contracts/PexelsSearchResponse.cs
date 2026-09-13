using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Images.Contracts;

public record PexelsSearchResponse
{
    [JsonPropertyName("photos")]
    public List<PexelsPhoto> Photos { get; init; } = [];
}