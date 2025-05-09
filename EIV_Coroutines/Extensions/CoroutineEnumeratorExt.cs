using System.Numerics;

namespace EIV_Coroutines.Extensions;

public static class CoroutineEnumeratorExt<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    #region Simple Enumerators
    public static IEnumerator<T> Empty()
    {
        yield return T.Zero;
    }

    public static IEnumerator<T> DelayedCall(TimeSpan timeSpan, Action action)
    {
        yield return CoroutineStaticExt<T>.ReturnTimeAsT(timeSpan.TotalSeconds);
        action();
    }

    public static IEnumerator<T> CallContinuously(TimeSpan timeSpan, Action action)
    {
        while (true)
        {
            yield return CoroutineStaticExt<T>.ReturnTimeAsT(timeSpan.TotalSeconds);
            action();
        }
    }
    #endregion
    #region StartWhenDone
    public static IEnumerator<T> StartWhenDone(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<T> pausedProc)
    {
        if (evaluatorFunc == null)
            yield break;
        while (evaluatorFunc() == continueOn)
        {
            yield return T.NegativeInfinity;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = pausedProc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    public static IEnumerator<T> StartWhenTDone<Number>(Func<Number>? evaluatorFunc, Number continueOn, IEnumerator<T> pausedProc) where Number : INumber<Number>
    {
        if (evaluatorFunc == null)
            yield break;
        while (evaluatorFunc() != continueOn)
        {
            yield return T.NegativeInfinity;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = pausedProc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    public static IEnumerator<T> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<T> pausedProc)
    {
        if (!coroutine.HasValue)
            yield break;
        while (CoroutineStaticExt<T>.StaticWorker!.IsCoroutineSuccessInstance(coroutine.Value))
        {
            yield return T.NegativeInfinity;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = pausedProc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    #endregion
    #region Helpers
    public static IEnumerator<T> ReturnTmpRefForRepFunc(IEnumerator<T> coptr)
    {
        CoroutineStaticExt<T>.StartIfNotExists();
        if (CoroutineStaticExt<T>.StaticWorker!.ReplacementObject == null)
            return Empty();
        if (CoroutineStaticExt<T>.StaticWorker!.ReplacementObject is IEnumerator<T> that && that != null)
            return that;
        return Empty();
    }

    public static IEnumerator<T> WaitUntilFalseHelper(IEnumerator<T> coptr)
    {
        CoroutineStaticExt<T>.StartIfNotExists();
        return StartWhenDone(CoroutineStaticExt<T>.StaticWorker!.ReplacementObject as Func<bool>, true, coptr);
    }

    public static IEnumerator<T> WaitUntilTrueHelper(IEnumerator<T> coptr)
    {
        CoroutineStaticExt<T>.StartIfNotExists();
        return StartWhenDone(CoroutineStaticExt<T>.StaticWorker!.ReplacementObject as Func<bool>, false, coptr);
    }

    public static IEnumerator<T> WaitUntilTHelper<Number>(IEnumerator<T> coptr) where Number : INumber<Number>
    {
        CoroutineStaticExt<T>.StartIfNotExists();
        return StartWhenTDone<Number>(CoroutineStaticExt<T>.StaticWorker!.ReplacementObject as Func<Number>, Number.Zero, coptr);
    }

    public static IEnumerator<T> StartAfterCoroutineHelper(IEnumerator<T> coptr)
    {
        CoroutineStaticExt<T>.StartIfNotExists();
        return StartWhenDone((CoroutineHandle?)CoroutineStaticExt<T>.StaticWorker!.ReplacementObject, coptr);
    }

    #endregion
}
