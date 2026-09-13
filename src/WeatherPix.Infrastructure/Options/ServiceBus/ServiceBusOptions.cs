using System.ComponentModel.DataAnnotations;

namespace WeatherPix.Infrastructure.Options.ServiceBus;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    [Required]
    public required string FullyQualifiedNamespace { get; init; }

    [Required]
    public required string StartJobsQueueName { get; init; }

    [Required]
    public required string ImageJobsQueueName { get; init; }
}