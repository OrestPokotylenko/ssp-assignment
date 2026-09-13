using WeatherPix.Domain.Image;
using WeatherPix.Domain.Job;

namespace WeatherPix.Functions.Jobs.Contracts;

public record JobResultsResponse(
    Guid OperationId,
    JobStatus Status,
    IReadOnlyCollection<GeneratedImage> Images
);