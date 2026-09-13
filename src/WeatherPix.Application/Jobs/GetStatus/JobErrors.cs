using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetStatus;

public static class JobErrors
{
    public static readonly Error StatusRetrievalFailed = new(
        "Job.StatusRetrievalFailed",
        "Failed to retrieve job status.");
}