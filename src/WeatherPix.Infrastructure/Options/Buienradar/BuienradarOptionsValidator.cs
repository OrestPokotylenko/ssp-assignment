using Microsoft.Extensions.Options;

namespace WeatherPix.Infrastructure.Options.Buienradar;

[OptionsValidator]
public partial class BuienradarOptionsValidator : IValidateOptions<BuienradarOptions>
{
}