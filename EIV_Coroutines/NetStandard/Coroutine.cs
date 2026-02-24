#if NETSTANDARD2_0
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Represent a Coroutine instance.
/// </summary>
public class Coroutine
{
    private static int Increment;

    /// <summary>
    /// The current delay the coroutine should wait until running again.
    /// </summary>
    public float Delay { get; internal set; } = 0f;

    /// <summary>
    /// The tag for distinge between other coroutines
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// The Enumerator that was created.
    /// </summary>
    public IEnumerator<float> BaseEnumerator { get; }

    /// <summary>
    /// The current enumerator.
    /// </summary>
    public IEnumerator<float> Enumerator;

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
    public Coroutine(IEnumerator<float> enumerator, string tag = "")
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

    public bool Equals(Coroutine? x, Coroutine? y)
    {
        return x?.GetHashCode() == y?.GetHashCode();
    }

    public int GetHashCode(Coroutine obj)
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
        return obj is Coroutine coroutine && Equals(coroutine);
    }

    public bool Equals(Coroutine? other)
    {
        return GetHashCode() == other?.GetHashCode();
    }


    /// <inheritdoc/>
    public static bool operator ==(Coroutine? left, Coroutine? right)
    {
        if (left is null)
            return false;
        return left.Equals(right);
    }


    /// <inheritdoc/>
    public static bool operator !=(Coroutine? left, Coroutine? right)
    {
        return !(left == right);
    }
}
#endif