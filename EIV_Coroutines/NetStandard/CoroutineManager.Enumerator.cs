#if NETSTANDARD2_0
namespace EIV_Coroutines;

public partial class CoroutineManager
{
    /// <summary>
    /// An empty coroutine.
    /// </summary>
    public static IEnumerator<float> Empty()
    {
        yield return 0f;
    }

    /// <summary>
    /// A delayed call coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    public static IEnumerator<float> DelayedCall(TimeSpan timeSpan, Action action)
    {
        yield return (float)timeSpan.TotalSeconds;
        action();
    }

    /// <summary>
    /// A continous calling coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    public static IEnumerator<float> CallContinuously(TimeSpan timeSpan, Action action)
    {
        while (true)
        {
            yield return (float)timeSpan.TotalSeconds;
            action();
        }
    }

    /// <summary>
    /// A helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
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

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after the coroutine is finished.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <param name="pausedProc">The paused process.</param>
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

    /// <summary>
    /// The replacement function to replace the current coroutine to <paramref name="coptr"/>.
    /// </summary>
    /// <param name="coptr">The coroutine to replace to.</param>
    public static IEnumerator<float> ReturnTmpRefForRepFunc(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        if (StaticWorker!.ReplacementObject == null)
            return Empty();
        if (StaticWorker!.ReplacementObject is IEnumerator<float> that && that != null)
            return that;
        return Empty();
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilFalse(Func{bool})"/>.
    /// </summary>
    public static IEnumerator<float> WaitUntilFalseHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, true, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilTrue(Func{bool})"/>.
    /// </summary>
    public static IEnumerator<float> WaitUntilTrueHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, false, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="StartAfterCoroutine(CoroutineHandle)"/>.
    /// </summary>
    public static IEnumerator<float> StartAfterCoroutineHelper(IEnumerator<float> coptr)
    {
        StartIfNotExists();
        return StartWhenDone((CoroutineHandle?)StaticWorker!.ReplacementObject, coptr);
    }
}
#endif