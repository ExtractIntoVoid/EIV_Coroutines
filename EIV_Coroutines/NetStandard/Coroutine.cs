#if NETSTANDARD2_0
namespace EIV_Coroutines;

/// <summary>
/// Represent a Coroutine instance.
/// </summary>
public class Coroutine :
    IEquatable<Coroutine>,
    IEqualityComparer<Coroutine>
{
    private static int increment;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool operator ==(Coroutine? left, Coroutine? right)
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
    public static bool operator !=(Coroutine? left, Coroutine? right)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return !(left == right);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Coroutine"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    /// <param name="tag">The tag to identify.</param>
    public Coroutine(IEnumerator<float> enumerator, string tag = "")
    {
        Interlocked.Increment(ref increment);
        Enumerator = enumerator;
        BaseEnumerator = enumerator;
        Tag = tag;
    }

    /// <summary>
    /// Gets or sets the current delay the coroutine should wait until running again.
    /// </summary>
    public float Delay { get; set; } = 0;

    /// <summary>
    /// Gets the tag for distinge between other coroutines.
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// Gets the Enumerator that was created.
    /// </summary>
    public IEnumerator<float> BaseEnumerator { get; }

    /// <summary>
    /// Gets or sets the current enumerator.
    /// </summary>
    public IEnumerator<float> Enumerator { get; set; }

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
    public bool Equals(Coroutine? x, Coroutine? y)
    {
        return x?.GetHashCode() == y?.GetHashCode();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool Equals(Coroutine? other)
    {
        return GetHashCode() == other?.GetHashCode();
    }
}
#endif