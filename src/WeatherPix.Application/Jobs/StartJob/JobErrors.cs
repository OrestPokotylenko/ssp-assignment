using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.StartJob;

public static class JobErrors
{
    public static readonly Error StatusUpdateFailed = new(
        "Job.StatusUpdateFailed",
        "Failed to update the job status.");

    public static readonly Error WeatherRetrievalFailed = new(
        "Job.WeatherRetrievalFailed",
        "Failed to retrieve weather station data.");

    public static readonly Error ImageQueueFailed = new(
        "Job.ImageQueueFailed",
        "Failed to queue image generation jobs.");
}