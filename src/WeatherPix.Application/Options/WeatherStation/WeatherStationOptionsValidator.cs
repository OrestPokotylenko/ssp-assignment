using Microsoft.Extensions.Options;

namespace WeatherPix.Application.Options.WeatherStation;

[OptionsValidator]
public partial class WeatherStationOptionsValidator : IValidateOptions<WeatherStationOptions>
{
}