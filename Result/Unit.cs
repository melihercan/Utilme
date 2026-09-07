using System;

namespace Utilme;

/// <summary>
/// A type with exactly one value, used as the payload of a <see cref="Result{T}"/> for operations
/// that succeed without producing anything.
/// </summary>
/// <remarks>
/// Use <c>Result&lt;Unit&gt;</c> where you would otherwise want a non-generic result, and create
/// one with <see cref="Result.Ok()"/>.
/// </remarks>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>The single value of this type.</summary>
    public static Unit Value => default;

    /// <inheritdoc/>
    public bool Equals(Unit other) => true;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc/>
    public override int GetHashCode() => 0;

    /// <inheritdoc/>
    public override string ToString() => "()";

    /// <summary>All instances are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>All instances are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(Unit left, Unit right) => false;
}
