#if NETSTANDARD2_0
namespace EIV_Coroutines;

/// <summary>
/// Represent a handle of a given coroutine.
/// </summary>
/// <param name="hash">The coroutine hash.</param>
public readonly struct CoroutineHandle(int hash) :
    IEquatable<CoroutineHandle>,
    IEqualityComparer<CoroutineHandle>
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static implicit operator int(CoroutineHandle coroutineHandle)
    {
        return coroutineHandle.CoroutineHash;
    }

    public static implicit operator CoroutineHandle(Coroutine coroutine)
    {
        return AsHandle(coroutine);
    }

    public static bool operator ==(CoroutineHandle left, CoroutineHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CoroutineHandle left, CoroutineHandle right)
    {
        return !(left == right);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Gets the <paramref name="coroutine"/> as a <see cref="CoroutineHandle"/>.
    /// </summary>
    /// <param name="coroutine">The given coroutine.</param>
    /// <returns>A new handle.</returns>
    public static CoroutineHandle AsHandle(Coroutine coroutine)
    {
        return new CoroutineHandle(coroutine.GetHashCode());
    }

    /// <summary>
    /// Gets the hash of the current coroutine.
    /// </summary>
    public int CoroutineHash { get; } = hash;

    /// <inheritdoc/>
    public bool Equals(CoroutineHandle other)
    {
        return CoroutineHash == other.CoroutineHash;
    }

    /// <inheritdoc/>
    public bool Equals(CoroutineHandle x, CoroutineHandle y)
    {
        return x.CoroutineHash == y.CoroutineHash;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return CoroutineHash;
    }

    /// <inheritdoc/>
    public int GetHashCode(CoroutineHandle obj)
    {
        return obj.CoroutineHash;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CoroutineHandle handle && Equals(handle);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Hash: {CoroutineHash}";
    }
}
#endif