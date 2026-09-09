using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Functions.Helpers.Auth.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public required string Domain { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Required]
    public required string ReadPermission { get; init; }

    [Required]
    public required string CreatePermission { get; init; }
}