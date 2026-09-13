using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.QueueJob;

public static class JobErrors
{
    public static readonly Error CreationFailed = new(
    "Job.CreationFailed",
    "Failed to create the job."
    );

    public static readonly Error QueueFailed = new(
        "Job.QueueFailed",
        "The job was created, but could not be queued."
    );
}