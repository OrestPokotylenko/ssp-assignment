using System.Text.Json.Serialization;

namespace WeatherPix.Infrastructure.Images.Contracts;

public record PexelsPhoto
{
    [JsonPropertyName("src")]
    public required PexelsPhotoSource Src { get; init; }
}