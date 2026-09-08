using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Infrastructure.Options.Pexels;

public sealed class PexelsOptions
{
    public const string SectionName = "Pexels";

    [Required]
    public required string ApiKey { get; init; }

    [Required]
    public required string BaseUrl { get; init; }
}