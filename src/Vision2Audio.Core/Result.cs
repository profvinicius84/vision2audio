namespace Vision2Audio.Core;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public readonly record struct Result<T>(bool IsSuccess, T? Value, string? Error)
{
    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failed result.</summary>
    public static Result<T> Failure(string error) => new(false, default, error);
}
