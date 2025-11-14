using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Represent a Coroutine instance.
/// </summary>
/// <typeparam name="T">Any floating point type.</typeparam>
public class Coroutine<T> : 
    IEquatable<Coroutine<T>>, 
    IEqualityComparer<Coroutine<T>> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    private static int Increment;

    /// <summary>
    /// The current delay the coroutine should wait until running again.
    /// </summary>
    public T Delay { get; internal set; } = T.Zero;

    /// <summary>
    /// The tag for distinge between other coroutines
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// The Enumerator that was created.
    /// </summary>
    public IEnumerator<T> BaseEnumerator { get; }

    /// <summary>
    /// The current enumerator.
    /// </summary>
    public IEnumerator<T> Enumerator;

    /// <summary>
    /// Gets whenever this coroutine is running.
    /// </summary>
    public bool IsRunning = true;

    /// <summary>
    /// Gets whenever this coroutine is paused.
    /// </summary>
    public bool IsPaused;

    /// <summary>
    /// Gets whenever this coroutine is being killed.
    /// </summary>
    public bool ShouldKill;

    /// <summary>
    /// Gets whenever this coroutine is success.
    /// </summary>
    public bool IsSuccess;

    /// <summary>
    /// Creates a Coroutine with the given paremeters.
    /// </summary>
    /// <param name="enumerator"></param>
    /// <param name="tag"></param>
    public Coroutine(IEnumerator<T> enumerator, string tag = "")
    {
        Interlocked.Increment(ref Increment);
        Enumerator = enumerator;
        BaseEnumerator = enumerator;
        Tag = tag;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return BaseEnumerator != null ? 
            BaseEnumerator.GetHashCode() + Tag.GetHashCode() + Increment : 
            0;
    }

    public bool Equals(Coroutine<T>? x, Coroutine<T>? y)
    {
        return x?.GetHashCode() == y?.GetHashCode();
    }

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

    public bool Equals(Coroutine<T>? other)
    {
        return GetHashCode() == other?.GetHashCode();
    }


    /// <inheritdoc/>
    public static bool operator ==(Coroutine<T>? left, Coroutine<T>? right)
    {
        if (left is null)
            return false;
        return left.Equals(right);
    }


    /// <inheritdoc/>
    public static bool operator !=(Coroutine<T>? left, Coroutine<T>? right)
    {
        return !(left == right);
    }
}