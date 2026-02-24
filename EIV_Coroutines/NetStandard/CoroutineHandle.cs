#if NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EIV_Coroutines;

public readonly struct CoroutineHandle(int hash) : 
    IEquatable<CoroutineHandle>, 
    IEqualityComparer<CoroutineHandle>
{
    public int CoroutineHash { get; } = hash;

    public bool Equals(CoroutineHandle other)
    {
        return CoroutineHash == other.CoroutineHash;
    }

    public bool Equals(CoroutineHandle x, CoroutineHandle y)
    {
        return x.CoroutineHash == y.CoroutineHash;
    }

    public override int GetHashCode()
    {
        return CoroutineHash;
    }

    public int GetHashCode(CoroutineHandle obj)
    {
        return obj.CoroutineHash;
    }


    public static implicit operator int(CoroutineHandle coroutineHandle)
    {
        return coroutineHandle.CoroutineHash;
    }

    public static implicit operator CoroutineHandle(Coroutine coroutine)
    {
        return new CoroutineHandle(coroutine.GetHashCode());
    }


    public static CoroutineHandle AsHandle(Coroutine coroutine)
    {
        return new CoroutineHandle(coroutine.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is CoroutineHandle handle && Equals(handle);
    }

    public static bool operator ==(CoroutineHandle left, CoroutineHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CoroutineHandle left, CoroutineHandle right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"Hash: {CoroutineHash}";
    }
}
#endif