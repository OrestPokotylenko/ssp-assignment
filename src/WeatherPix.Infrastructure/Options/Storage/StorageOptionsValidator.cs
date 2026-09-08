using Microsoft.Extensions.Options;

namespace WeatherPix.Infrastructure.Options.Storage;

[OptionsValidator]
public partial class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
}