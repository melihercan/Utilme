using System;
using System.Threading.Tasks;

namespace Utilme.Functional;

/// <summary>
/// Composition operators for <see cref="Result{T}"/>: transform, chain and unwrap results without
/// hand-written <c>if (result.IsOk)</c> ladders.
/// </summary>
/// <remarks>
/// <para>
/// These live in their own namespace so that importing them is a deliberate act. Bringing them in
/// with <c>using Utilme.Functional;</c> cannot collide with same-named extension methods an
/// application may already define on <see cref="Result{T}"/> unless both namespaces are imported
/// into the same file.
/// </para>
/// <para>
/// Every operator short-circuits: when the input already failed, the delegate is not invoked and
/// the failure is propagated with its <see cref="Result{T}.Status"/> and
/// <see cref="Result{T}.ErrorMessage"/> preserved exactly.
/// </para>
/// </remarks>
public static class ResultExtensions
{
    // ------------------------------------------------------------------ unwrap

    /// <summary>Collapses a result into a single value by handling both outcomes.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <typeparam name="TOut">The type produced by both branches.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="onOk">Invoked with the value when the result succeeded.</param>
    /// <param name="onError">Invoked with the failed result otherwise.</param>
    public static TOut Match<T, TOut>(
        this Result<T> result, Func<T, TOut> onOk, Func<Result<T>, TOut> onError)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        return result.IsOk ? onOk(result.Value!) : onError(result);
    }

    /// <summary>Runs one of two side effects depending on the outcome.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="onOk">Invoked with the value when the result succeeded.</param>
    /// <param name="onError">Invoked with the failed result otherwise.</param>
    /// <remarks>
    /// Named differently from <see cref="Match{T, TOut}"/> so that a lambda whose body is a method
    /// call cannot be ambiguous between the two.
    /// </remarks>
    public static void Switch<T>(
        this Result<T> result, Action<T> onOk, Action<Result<T>> onError)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onOk);
        ArgumentNullException.ThrowIfNull(onError);

        if (result.IsOk)
        {
            onOk(result.Value!);
        }
        else
        {
            onError(result);
        }
    }

    /// <summary>
    /// Returns the value on success, or the result of <paramref name="fallback"/> on failure.
    /// </summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <param name="result">The result to unwrap.</param>
    /// <param name="fallback">Invoked with the failed result to produce a substitute value.</param>
    public static T Else<T>(this Result<T> result, Func<Result<T>, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fallback);

        return result.IsOk ? result.Value! : fallback(result);
    }

    // --------------------------------------------------------------- transform

    /// <summary>Transforms the value of a successful result, propagating failure untouched.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="map">Invoked with the value when the result succeeded.</param>
    public static Result<TOut> Map<T, TOut>(this Result<T> result, Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        return result.IsOk
            ? Result<TOut>.Ok(map(result.Value!))
            : Result<TOut>.FromFailure(result.Status, result.ErrorMessage);
    }

    /// <summary>
    /// Chains an operation that itself returns a result, propagating failure untouched.
    /// </summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="next">Invoked with the value when the result succeeded.</param>
    public static Result<TOut> Then<T, TOut>(this Result<T> result, Func<T, Result<TOut>> next)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(next);

        return result.IsOk
            ? next(result.Value!)
            : Result<TOut>.FromFailure(result.Status, result.ErrorMessage);
    }

    // ------------------------------------------------------------ side effects

    /// <summary>Runs a side effect on success and returns the original result.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">Invoked with the value when the result succeeded.</param>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsOk)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>Runs a side effect on failure and returns the original result.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">Invoked with the failed result.</param>
    public static Result<T> TapError<T>(this Result<T> result, Action<Result<T>> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsError)
        {
            action(result);
        }

        return result;
    }

    // ------------------------------------------------------------------- async

    /// <summary>Asynchronous <see cref="Map{T, TOut}"/>.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="map">Invoked with the value when the result succeeded.</param>
    public static async Task<Result<TOut>> MapAsync<T, TOut>(
        this Result<T> result, Func<T, Task<TOut>> map)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        return result.IsOk
            ? Result<TOut>.Ok(await map(result.Value!).ConfigureAwait(false))
            : Result<TOut>.FromFailure(result.Status, result.ErrorMessage);
    }

    /// <summary>Asynchronous <see cref="Then{T, TOut}"/>.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="next">Invoked with the value when the result succeeded.</param>
    public static async Task<Result<TOut>> ThenAsync<T, TOut>(
        this Result<T> result, Func<T, Task<Result<TOut>>> next)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(next);

        return result.IsOk
            ? await next(result.Value!).ConfigureAwait(false)
            : Result<TOut>.FromFailure(result.Status, result.ErrorMessage);
    }

    /// <summary>Continues a chain from an awaited result.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The pending result to transform.</param>
    /// <param name="map">Invoked with the value when the result succeeded.</param>
    public static async Task<Result<TOut>> MapAsync<T, TOut>(
        this Task<Result<T>> result, Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(result);

        return (await result.ConfigureAwait(false)).Map(map);
    }

    /// <summary>Continues a chain from an awaited result.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The pending result to chain from.</param>
    /// <param name="next">Invoked with the value when the result succeeded.</param>
    public static async Task<Result<TOut>> ThenAsync<T, TOut>(
        this Task<Result<T>> result, Func<T, Task<Result<TOut>>> next)
    {
        ArgumentNullException.ThrowIfNull(result);

        return await (await result.ConfigureAwait(false)).ThenAsync(next).ConfigureAwait(false);
    }

    /// <summary>Collapses an awaited result into a single value.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <typeparam name="TOut">The type produced by both branches.</typeparam>
    /// <param name="result">The pending result to inspect.</param>
    /// <param name="onOk">Invoked with the value when the result succeeded.</param>
    /// <param name="onError">Invoked with the failed result otherwise.</param>
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Task<Result<T>> result, Func<T, TOut> onOk, Func<Result<T>, TOut> onError)
    {
        ArgumentNullException.ThrowIfNull(result);

        return (await result.ConfigureAwait(false)).Match(onOk, onError);
    }

    /// <summary>Runs a side effect on success of an awaited result.</summary>
    /// <typeparam name="T">The result's value type.</typeparam>
    /// <param name="result">The pending result to inspect.</param>
    /// <param name="action">Invoked with the value when the result succeeded.</param>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(result);

        return (await result.ConfigureAwait(false)).Tap(action);
    }
}
