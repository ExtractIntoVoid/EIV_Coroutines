#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Represent a handle of a given coroutine.
/// </summary>
/// <param name="hash">The coroutine hash.</param>
public readonly struct CoroutineHandle(int hash) : 
    IEquatable<CoroutineHandle>, 
    IEqualityComparer<CoroutineHandle>
{
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
    public int GetHashCode([DisallowNull] CoroutineHandle obj)
    {
        return obj.CoroutineHash;
    }

    /// <inheritdoc/>
    public static implicit operator int(CoroutineHandle coroutineHandle)
    {
        return coroutineHandle.CoroutineHash;
    }

    /// <inheritdoc/>
    public static implicit operator CoroutineHandle(Coroutine<double> coroutine)
    {
        return AsHandle(coroutine);
    }

    /// <inheritdoc/>
    public static implicit operator CoroutineHandle(Coroutine<float> coroutine)
    {
        return AsHandle(coroutine);
    }

    /// <summary>
    /// Gets the <paramref name="coroutine"/> as a <see cref="CoroutineHandle"/>.
    /// </summary>
    /// <typeparam name="T">A floating point type.</typeparam>
    /// <param name="coroutine">The given coroutine.</param>
    /// <returns>A new handle.</returns>
    public static CoroutineHandle AsHandle<T>(Coroutine<T> coroutine) where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return new CoroutineHandle(coroutine.GetHashCode());
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CoroutineHandle handle && Equals(handle);
    }

    /// <inheritdoc/>
    public static bool operator ==(CoroutineHandle left, CoroutineHandle right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(CoroutineHandle left, CoroutineHandle right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Hash: {CoroutineHash}";
    }
}
#endif