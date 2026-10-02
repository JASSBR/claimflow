namespace ClaimFlow.SharedKernel;

/// <summary>
/// Expected business failures travel as values; exceptions stay reserved for the unexpected.
/// Several errors can be returned at once so a form can show every invalid field in one round trip.
/// </summary>
public class Result
{
    protected Result(IReadOnlyList<Error> errors) => Errors = errors;

    public IReadOnlyList<Error> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new([]);

    public static Result<T> Success<T>(T value) => new(value, []);

    public static Result Failure(params Error[] errors) => new(EnsureNotEmpty(errors));

    public static Result<T> Failure<T>(params Error[] errors) => new(default, EnsureNotEmpty(errors));

    public static implicit operator Result(Error error) => Failure(error);

    private static Error[] EnsureNotEmpty(Error[] errors) =>
        errors.Length > 0 ? errors : throw new ArgumentException("A failure needs at least one error.", nameof(errors));
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, IReadOnlyList<Error> errors)
        : base(errors) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
