using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Utilme;

/// <summary>
/// The outcome of an operation: either a value, when <see cref="IsOk"/> is <see langword="true"/>,
/// or a failure described by <see cref="Status"/> and <see cref="ErrorMessage"/>.
/// </summary>
/// <typeparam name="T">Type of the value carried on success.</typeparam>
/// <remarks>
/// <para>
/// Instances are immutable. Failures that carry no caller-supplied message are cached and shared
/// per closed generic type, so two reads of <see cref="NotFound"/> return the same object.
/// </para>
/// <para>
/// Two long-standing behaviours are deliberately preserved for backward compatibility, and are
/// documented on the members concerned: <see cref="Value"/> returns <see langword="default"/>
/// rather than throwing on a failed result, and <see cref="NetworkUp"/> reports a
/// <em>failure</em> despite its name.
/// </para>
/// <para>
/// The per-status members below come in two shapes — <see cref="NotFound"/> is a property while
/// <see cref="NotSupported"/> is a method — because that is how they were originally published and
/// C# forbids one name being both. Prefer <see cref="Fail"/>, or the factories on the
/// <see cref="Result"/> class, in new code.
/// </para>
/// </remarks>
public class Result<T>
{
    // Failures carrying no caller-supplied message are immutable and stateless, so one shared
    // instance per closed generic type is enough. Ok/Error still allocate: they carry data.
    static readonly Result<T> _error = new(ResultStatus.Error);
    static readonly Result<T> _notFound = new(ResultStatus.NotFound);
    static readonly Result<T> _timeout = new(ResultStatus.Timeout);
    static readonly Result<T> _cancelled = new(ResultStatus.Cancelled);
    static readonly Result<T> _notSupported = new(ResultStatus.NotSupported);
    static readonly Result<T> _invalidData = new(ResultStatus.InvalidData);
    static readonly Result<T> _networkUp = new(ResultStatus.NetworkUp);
    static readonly Result<T> _networkDown = new(ResultStatus.NetworkDown);

    /// <summary>The value produced by a successful operation.</summary>
    /// <remarks>
    /// On a failed result this returns <see langword="default"/> and does <em>not</em> throw, so
    /// check <see cref="IsOk"/> before relying on it, or use <see cref="TryGetValue"/> /
    /// <see cref="GetValueOrDefault"/>. A successful result may also legitimately carry
    /// <see langword="null"/>.
    /// </remarks>
    public T? Value { get; }

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsOk { get; }

    /// <summary>Whether the operation failed. The inverse of <see cref="IsOk"/>.</summary>
    public bool IsError => !IsOk;

    /// <summary>The status of the operation. <see cref="ResultStatus.Ok"/> on success.</summary>
    public ResultStatus Status { get; }

    /// <summary>
    /// A description of the failure, or <see langword="null"/> on success.
    /// </summary>
    /// <remarks>
    /// For failures created from a <see cref="ResultStatus"/> without an explicit message this is
    /// simply the status name, so it carries no additional context. Use
    /// <see cref="Fail"/> to attach a real description.
    /// </remarks>
    public string? ErrorMessage { get; }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value produced by the operation. May be <see langword="null"/>.</param>
    public Result(T value)
    {
        Value = value;
        IsOk = true;
        Status = ResultStatus.Ok;
    }

    /// <summary>Creates a failure whose message is the status name.</summary>
    Result(ResultStatus status)
    {
        Status = status;
        IsOk = false;
        ErrorMessage = status.ToString();
    }

    /// <summary>Creates a failure carrying <paramref name="message"/> verbatim, null included.</summary>
    Result(ResultStatus status, string? message)
    {
        Status = status;
        IsOk = false;
        ErrorMessage = message;
    }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value produced by the operation. May be <see langword="null"/>.</param>
    public static Result<T> Ok(T value) => new Result<T>(value);

    /// <summary>Creates a failed result carrying <paramref name="message"/>.</summary>
    /// <param name="message">A description of what went wrong.</param>
    /// <remarks>
    /// <see cref="Status"/> is always <see cref="ResultStatus.Error"/>. To pair a message with a
    /// more specific status, use <see cref="Fail"/>.
    /// </remarks>
    public static Result<T> Error(string message) => new Result<T>(ResultStatus.Error, message);

    /// <summary>
    /// Creates a failed result with <paramref name="status"/> and an optional
    /// <paramref name="message"/>.
    /// </summary>
    /// <param name="status">The failure status. Must not be <see cref="ResultStatus.Ok"/>.</param>
    /// <param name="message">
    /// A description of what went wrong. When <see langword="null"/> the status name is used and a
    /// shared cached instance is returned.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="status"/> is <see cref="ResultStatus.Ok"/>, which is not a failure.
    /// </exception>
    /// <remarks>
    /// This is the general-purpose failure factory, and the only one that can pair a specific
    /// status with a caller-supplied message. Prefer it in new code.
    /// </remarks>
    public static Result<T> Fail(ResultStatus status, string? message = null)
    {
        if (status == ResultStatus.Ok)
        {
            throw new ArgumentOutOfRangeException(
                nameof(status), status, "Ok is not a failure status. Use Ok(value) instead.");
        }

        return message is null ? Cached(status) : new Result<T>(status, message);
    }

    /// <summary>A failed result with <see cref="ResultStatus.NotFound"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> NotFound => _notFound;

    /// <summary>A failed result with <see cref="ResultStatus.Timeout"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> Timeout => _timeout;

    /// <summary>A failed result with <see cref="ResultStatus.Cancelled"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> Cancelled => _cancelled;

    /// <summary>Returns a failed result with <see cref="ResultStatus.NotSupported"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> NotSupported() => _notSupported;

    /// <summary>Returns a failed result with <see cref="ResultStatus.InvalidData"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> InvalidData() => _invalidData;

    /// <summary>Returns a failed result with <see cref="ResultStatus.NetworkUp"/>.</summary>
    /// <remarks>
    /// <para>
    /// Despite the name this is a <em>failure</em>: <see cref="IsOk"/> is <see langword="false"/>.
    /// The polarity is preserved for backward compatibility; changing it would silently invert
    /// branches in existing callers.
    /// </para>
    /// <para>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> NetworkUp() => _networkUp;

    /// <summary>Returns a failed result with <see cref="ResultStatus.NetworkDown"/>.</summary>
    /// <remarks>
    /// Hidden from IntelliSense. The per-status members are inconsistently shaped — some are
    /// properties, some are methods — and cannot be unified without a breaking removal, so new code
    /// should prefer <see cref="Fail"/> or the factories on the <see cref="Result"/> class. This
    /// member remains fully supported.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> NetworkDown() => _networkDown;

    /// <summary>Gets the value when the operation succeeded.</summary>
    /// <param name="value">
    /// The value on success; otherwise <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> when the result is successful.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = Value;
        return IsOk;
    }

    /// <summary>
    /// Returns the value on success, or <paramref name="fallback"/> on failure.
    /// </summary>
    /// <param name="fallback">The value to substitute when the operation failed.</param>
    public T GetValueOrDefault(T fallback) => IsOk ? Value! : fallback;

    /// <summary>Returns a short, human-readable description, intended for logs.</summary>
    public override string ToString() =>
        IsOk
            ? $"Ok({Value})"
            : ErrorMessage is null || ErrorMessage == Status.ToString()
                ? Status.ToString()
                : $"{Status}: {ErrorMessage}";

    /// <summary>
    /// Rebuilds a failure of this type from another result's status and message, preserving both
    /// exactly. Used to carry a failure across a change of value type.
    /// </summary>
    internal static Result<T> FromFailure(ResultStatus status, string? message) =>
        message == status.ToString() ? Cached(status) : new Result<T>(status, message);

    /// <summary>
    /// The shared instance for a message-less failure. Falls back to allocating so that a status
    /// appended to <see cref="ResultStatus"/> keeps working without being registered here.
    /// </summary>
    static Result<T> Cached(ResultStatus status) => status switch
    {
        ResultStatus.Error => _error,
        ResultStatus.NotFound => _notFound,
        ResultStatus.Timeout => _timeout,
        ResultStatus.Cancelled => _cancelled,
        ResultStatus.NotSupported => _notSupported,
        ResultStatus.InvalidData => _invalidData,
        ResultStatus.NetworkUp => _networkUp,
        ResultStatus.NetworkDown => _networkDown,
        _ => new Result<T>(status),
    };
}
