using Microsoft.Extensions.Options;

namespace WeatherPix.Infrastructure.Options.Pexels;

[OptionsValidator]
public partial class PexelsOptionsValidator : IValidateOptions<PexelsOptions>
{
}