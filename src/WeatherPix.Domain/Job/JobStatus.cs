namespace WeatherPix.Domain.Job;

public enum JobStatus
{
    Queued = 1,
    Processing,
    Succeeded,
    Failed
}