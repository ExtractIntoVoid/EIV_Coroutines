using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    public static IEnumerator<T> Empty()
    {
        yield return T.Zero;
    }

    public static IEnumerator<T> DelayedCall(TimeSpan timeSpan, Action action)
    {
        yield return ReturnTimeAsT(timeSpan.TotalSeconds);
        action();
    }

    public static IEnumerator<T> CallContinuously(TimeSpan timeSpan, Action action)
    {
        while (true)
        {
            yield return ReturnTimeAsT(timeSpan.TotalSeconds);
            action();
        }
    }

    public static IEnumerator<T> StartWhenDone(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<T> pausedProc)
    {
        if (evaluatorFunc == null)
            yield break;
        while (evaluatorFunc() == continueOn)
        {
            yield return T.NegativeInfinity;
        }
        StartIfNotExists();
        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
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
        StartIfNotExists();
        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    public static IEnumerator<T> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<T> pausedProc)
    {
        if (!coroutine.HasValue)
            yield break;
        StartIfNotExists();
        while (StaticWorker!.IsCoroutineSuccessInstance(coroutine.Value))
        {
            yield return T.NegativeInfinity;
        }
        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    public static IEnumerator<T> ReturnTmpRefForRepFunc(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        if (StaticWorker!.ReplacementObject == null)
            return Empty();
        if (StaticWorker!.ReplacementObject is IEnumerator<T> that && that != null)
            return that;
        return Empty();
    }

    public static IEnumerator<T> WaitUntilFalseHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, true, coptr);
    }

    public static IEnumerator<T> WaitUntilTrueHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, false, coptr);
    }

    public static IEnumerator<T> WaitUntilTHelper<Number>(IEnumerator<T> coptr) where Number : INumber<Number>
    {
        StartIfNotExists();
        return StartWhenTDone<Number>(StaticWorker!.ReplacementObject as Func<Number>, Number.Zero, coptr);
    }

    public static IEnumerator<T> StartAfterCoroutineHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone((CoroutineHandle?)StaticWorker!.ReplacementObject, coptr);
    }
}
