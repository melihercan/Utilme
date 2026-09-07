namespace Utilme;

/// <summary>
/// Factories for <see cref="Result{T}"/> that infer the value type from the argument, and offer a
/// single consistent shape for every status.
/// </summary>
/// <remarks>
/// <para>
/// <c>Result.Ok(42)</c> infers <c>Result&lt;int&gt;</c>, where <c>Result&lt;int&gt;.Ok(42)</c>
/// requires naming the type twice.
/// </para>
/// <para>
/// Every failure here is a method, unlike the mixed property/method members on
/// <see cref="Result{T}"/> itself. Prefer these in new code.
/// </para>
/// </remarks>
public static class Result
{
    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <typeparam name="T">Type of the value, inferred from <paramref name="value"/>.</typeparam>
    /// <param name="value">The value produced by the operation.</param>
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    /// <summary>
    /// Creates a successful result for an operation that produces no value.
    /// </summary>
    public static Result<Unit> Ok() => Result<Unit>.Ok(Unit.Value);

    /// <summary>
    /// Creates a failed result with <paramref name="status"/> and an optional
    /// <paramref name="message"/>.
    /// </summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    /// <param name="status">The failure status. Must not be <see cref="ResultStatus.Ok"/>.</param>
    /// <param name="message">A description of what went wrong, or <see langword="null"/>.</param>
    public static Result<T> Fail<T>(ResultStatus status, string? message = null) =>
        Result<T>.Fail(status, message);

    /// <summary>Creates a failed result with <see cref="ResultStatus.Error"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    /// <param name="message">A description of what went wrong.</param>
    public static Result<T> Error<T>(string message) => Result<T>.Error(message);

    /// <summary>Creates a failed result with <see cref="ResultStatus.NotFound"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> NotFound<T>() => Result<T>.NotFound;

    /// <summary>Creates a failed result with <see cref="ResultStatus.Timeout"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> Timeout<T>() => Result<T>.Timeout;

    /// <summary>Creates a failed result with <see cref="ResultStatus.Cancelled"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> Cancelled<T>() => Result<T>.Cancelled;

    /// <summary>Creates a failed result with <see cref="ResultStatus.NotSupported"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> NotSupported<T>() => Result<T>.NotSupported();

    /// <summary>Creates a failed result with <see cref="ResultStatus.InvalidData"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> InvalidData<T>() => Result<T>.InvalidData();

    /// <summary>Creates a failed result with <see cref="ResultStatus.NetworkUp"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    /// <remarks>Despite the name this is a failure; see <see cref="Result{T}.NetworkUp"/>.</remarks>
    public static Result<T> NetworkUp<T>() => Result<T>.NetworkUp();

    /// <summary>Creates a failed result with <see cref="ResultStatus.NetworkDown"/>.</summary>
    /// <typeparam name="T">Type the result would have carried on success.</typeparam>
    public static Result<T> NetworkDown<T>() => Result<T>.NetworkDown();
}
