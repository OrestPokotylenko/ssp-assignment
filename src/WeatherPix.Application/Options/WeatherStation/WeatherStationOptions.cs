using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Application.Options.WeatherStation;

public sealed class WeatherStationOptions
{
    public const string SectionName = "WeatherStation";

    [Required]
    public required int Count { get; init; }
}