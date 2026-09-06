namespace WeatherPix.Application.Common.Results;

public sealed class Result<T> : Result
{
    private Result(
        T? value,
        bool isSuccess,
        Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
        => new(value, true, null);

    public static Result<T> FailureWith(Error error)
        => new(default, false, error);
}