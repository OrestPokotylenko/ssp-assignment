using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Infrastructure.Options.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required]
    public required string TableServiceUri { get; init; }

    [Required]
    public required string JobStatusTableName { get; init; }

    [Required]
    public required string BlobServiceUri { get; init; }

    [Required]
    public required string GeneratedImagesContainerName { get; init; }
}