using WeatherPix.Domain.Image;
using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.GetResults;

public record JobResultsData(
    Guid OperationId,
    JobStatus Status,
    IReadOnlyCollection<GeneratedImage> Images);