using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Images;

public static class ImageErrors
{
    public static readonly Error GenerationFailed = new(
        "Image.GenerationFailed",
        "Failed to generate the weather image.");
}