using Microsoft.Extensions.Options;

namespace WeatherPix.Infrastructure.Options.ServiceBus;

[OptionsValidator]
public partial class ServiceBusOptionsValidator : IValidateOptions<ServiceBusOptions>
{
}