namespace WeatherPix.Domain.Jobs;

public enum JobStatus
{
    Queued,
    Processing,
    Succeeded,
    Failed
}