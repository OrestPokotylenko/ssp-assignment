using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Infrastructure.Options.Table;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required]
    public required string TableServiceUri { get; init; }

    [Required]
    public required string JobStatusTableName { get; init; }
}