namespace WeatherPix.Domain.Job;

public record JobProgress(
    int Total,
    int Succeeded,
    int Failed);