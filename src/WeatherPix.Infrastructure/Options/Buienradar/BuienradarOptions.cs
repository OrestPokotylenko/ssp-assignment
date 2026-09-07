using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Infrastructure.Options.Buienradar;

public sealed class BuienradarOptions
{
    public const string SectionName = "Buienradar";

    [Required]
    public required string BaseUrl { get; init; }
}