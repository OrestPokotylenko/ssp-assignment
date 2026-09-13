using Azure.Storage.Blobs.Models;

namespace WeatherPix.Infrastructure.Storage.Sas;

internal record SasContext(
    UserDelegationKey DelegationKey,
    DateTimeOffset StartsOn,
    DateTimeOffset ExpiresOn);