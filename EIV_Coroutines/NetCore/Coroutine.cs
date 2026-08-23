#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Represent a Coroutine instance.
/// </summary>
/// <typeparam name="T">Any floating point type.</typeparam>
public class Coroutine<T> :
    IEquatable<Coroutine<T>>,
    IEqualityComparer<Coroutine<T>>
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    private static int increment;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool operator ==(Coroutine<T>? left, Coroutine<T>? right)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null)
        {
            return false;
        }

        return left.Equals(right);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool operator !=(Coroutine<T>? left, Coroutine<T>? right)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return !(left == right);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coroutine{T}"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="tag">The tag to identify.</param>
    public Coroutine(IEnumerator<T> enumerator, string tag = "")
    {
        Interlocked.Increment(ref increment);
        Enumerator = enumerator;
        BaseEnumerator = enumerator;
        Tag = tag;
    }

    /// <summary>
    /// Gets or sets the current delay the coroutine should wait until running again.
    /// </summary>
    public T Delay { get; set; } = T.Zero;

    /// <summary>
    /// Gets the tag for distinge between other coroutines.
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// Gets the Enumerator that was created.
    /// </summary>
    public IEnumerator<T> BaseEnumerator { get; }

    /// <summary>
    /// Gets or sets the current enumerator.
    /// </summary>
    public IEnumerator<T> Enumerator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this coroutine is running.
    /// </summary>
    public bool IsRunning { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this coroutine is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this coroutine is being killed.
    /// </summary>
    public bool ShouldKill { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this coroutine is success.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return BaseEnumerator != null ?
            BaseEnumerator.GetHashCode() + Tag.GetHashCode() + increment :
            0;
    }

    /// <inheritdoc/>
    public bool Equals(Coroutine<T>? x, Coroutine<T>? y)
    {
        return x?.GetHashCode() == y?.GetHashCode();
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] Coroutine<T> obj)
    {
        return obj.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{GetHashCode()} IsRunning: {IsRunning}, ShouldKill {ShouldKill}, IsPaused: {IsPaused}, IsSuccess: {IsSuccess}, Tag: {Tag}, Delay: {Delay}";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Coroutine<T> coroutine && Equals(coroutine);
    }

    /// <inheritdoc/>
    public bool Equals(Coroutine<T>? other)
    {
        return GetHashCode() == other?.GetHashCode();
    }
}
#endif