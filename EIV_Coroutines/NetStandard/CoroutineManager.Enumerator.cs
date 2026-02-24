#if NETSTANDARD2_0
using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager
{
    public static IEnumerator<float> Empty()
    {
        yield return 0f;
    }

    public static IEnumerator<float> DelayedCall(TimeSpan timeSpan, Action action)
    {
        yield return (float)timeSpan.TotalSeconds;
        action();
    }

    public static IEnumerator<float> CallContinuously(TimeSpan timeSpan, Action action)
    {
        while (true)
        {
            yield return (float)timeSpan.TotalSeconds;
            action();
        }
    }

    public static IEnumerator<float> StartWhenDone(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<float> pausedProc)
    {
        if (evaluatorFunc == null)
            yield break;
        while (evaluatorFunc() == continueOn)
        {
            yield return float.NegativeInfinity;
        }
        StartIfNotExists();
        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return float.NaN;
    }

    public static IEnumerator<float> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<float> pausedProc)
    {
        if (!coroutine.HasValue)
            yield break;

        StartIfNotExists();
        /*
        // Return to original if no coroutine found.
        if (!StaticWorker!.IsCoroutineExistsInstance(coroutine.Value))
        {
            StaticWorker!.ReplacementObject = pausedProc;
            StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return T.NaN;
        }
        */
        while (!StaticWorker!.IsCoroutineSuccessInstance(coroutine.Value))
        {
            yield return float.NegativeInfinity;
        }

        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return float.NaN;
    }

    public static IEnumerator<float> ReturnTmpRefForRepFunc(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        if (StaticWorker!.ReplacementObject == null)
            return Empty();
        if (StaticWorker!.ReplacementObject is IEnumerator<float> that && that != null)
            return that;
        return Empty();
    }

    public static IEnumerator<float> WaitUntilFalseHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, true, coptr);
    }

    public static IEnumerator<float> WaitUntilTrueHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, false, coptr);
    }

    public static IEnumerator<float> StartAfterCoroutineHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone((CoroutineHandle?)StaticWorker!.ReplacementObject, coptr);
    }
}
#endif