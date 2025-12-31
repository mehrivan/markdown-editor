namespace Markdown.Domain.Primitives;

public static class Results
{
    public static Result Success()
    {
        return Result.CreateSuccess();
    }

    public static Result Failure(string error)
    {
        return Result.CreateFailure(error);
    }

    public static Result<T> Success<T>(T value)
    {
        return Result<T>.CreateSuccess(value);
    }

    public static Result<T> Failure<T>(string error)
    {
        return Result<T>.CreateFailure(error);
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("Successful result cannot have an error.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Failure result must have an error message.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    internal static Result CreateSuccess()
    {
        return new(true, null);
    }

    internal static Result CreateFailure(string error)
    {
        return new(false, error);
    }
}

public sealed class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    internal static Result<T> CreateSuccess(T value)
    {
        return new(true, value, null);
    }

    internal static new Result<T> CreateFailure(string error)
    {
        return new(false, default!, error);
    }
}