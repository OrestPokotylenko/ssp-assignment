using WeatherPix.Domain.Image;
using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.GetResults;

public record GetJobResultsResponse(
    Guid OperationId,
    JobStatus Status,
    IReadOnlyCollection<GeneratedImage> Images);