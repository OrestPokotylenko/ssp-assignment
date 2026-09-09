using Microsoft.Extensions.Options;
using WeatherPix.Functions.Helpers.Auth.Options;

namespace WeatherPix.Functions.Auth.Options;

[OptionsValidator]
public partial class AuthOptionsValidator : IValidateOptions<AuthOptions>
{
}