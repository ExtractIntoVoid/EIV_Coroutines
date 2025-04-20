using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EIV_Coroutines;

public class Coroutine<T>(IEnumerator<T> enumerator, string tag = "")
    : IEquatable<Coroutine<T>>, IEqualityComparer<Coroutine<T>> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    public IEnumerator<T> Enumerator = enumerator;
    public bool IsRunning = true;
    public bool IsPaused;
    public bool ShouldKill;
    public bool IsSuccess;
    public string Tag => tag;
    public IEnumerator<T> BaseEnumerator { get; } = enumerator;

    public override int GetHashCode()
    {
        return BaseEnumerator != null ? BaseEnumerator.GetHashCode() : 0;
    }

    public bool Equals(Coroutine<T>? x, Coroutine<T>? y)
    {
        return x?.GetHashCode() == y?.GetHashCode();
    }

    public int GetHashCode([DisallowNull] Coroutine<T> obj)
    {
        return obj.GetHashCode();
    }

    public override string ToString()
    {
        return $"{GetHashCode()} IsRunning: {IsRunning}, ShouldKill {ShouldKill}, IsPaused: {IsPaused}, IsSuccess: {IsSuccess}, Tag: {Tag}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Coroutine<T> && Equals((Coroutine<T>)obj);
    }

    public bool Equals(Coroutine<T>? other)
    {
        return GetHashCode() == other?.GetHashCode();
    }

    public static bool operator ==(Coroutine<T> left, Coroutine<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Coroutine<T> left, Coroutine<T> right)
    {
        return !(left == right);
    }
}