using Microsoft.Extensions.Options;

namespace WeatherPix.Infrastructure.Options.Table;

[OptionsValidator]
public partial class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
}