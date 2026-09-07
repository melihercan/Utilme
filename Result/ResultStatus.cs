namespace Utilme;

/// <summary>
/// The status of an operation reported by <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
/// The numeric values are part of the contract: callers may have persisted or transmitted them as
/// integers, so new members must only ever be <em>appended</em>, never inserted or reordered.
/// </remarks>
public enum ResultStatus
{
    /// <summary>The operation succeeded.</summary>
    Ok,

    /// <summary>The operation failed with a caller-supplied message.</summary>
    Error,

    /// <summary>The requested item does not exist.</summary>
    NotFound,

    /// <summary>The operation did not complete within its time budget.</summary>
    Timeout,

    /// <summary>The operation was cancelled before completing.</summary>
    Cancelled,

    /// <summary>The requested operation is not supported.</summary>
    NotSupported,

    /// <summary>The supplied data was malformed or invalid.</summary>
    InvalidData,

    /// <summary>
    /// The network came up. Note that results carrying this status report
    /// <see cref="Result{T}.IsOk"/> as <see langword="false"/>.
    /// </summary>
    NetworkUp,

    /// <summary>The network went down.</summary>
    NetworkDown,
}
