#if NET5_0_OR_GREATER
using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager<T> 
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    /// <summary>
    /// An empty coroutine.
    /// </summary>
    public static IEnumerator<T> Empty()
    {
        yield return T.Zero;
    }

    /// <summary>
    /// A delayed call coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    public static IEnumerator<T> DelayedCall(TimeSpan timeSpan, Action action)
    {
        yield return T.CreateChecked(timeSpan.TotalSeconds);
        action();
    }

    /// <summary>
    /// A continous calling coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    public static IEnumerator<T> CallContinuously(TimeSpan timeSpan, Action action)
    {
        while (true)
        {
            yield return T.CreateChecked(timeSpan.TotalSeconds);
            action();
        }
    }

    /// <summary>
    /// A helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
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

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
    public static IEnumerator<T> StartWhenZeroDone<Number>(Func<Number>? evaluatorFunc, Number continueOn, IEnumerator<T> pausedProc) where Number : INumber<Number>
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

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after the coroutine is finished.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <param name="pausedProc">The paused process.</param>
    public static IEnumerator<T> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<T> pausedProc)
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
            yield return T.NegativeInfinity;
        }

        StaticWorker!.ReplacementObject = pausedProc;
        StaticWorker!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    /// <summary>
    /// The replacement function to replace the current coroutine to <paramref name="coptr"/>.
    /// </summary>
    /// <param name="coptr">The coroutine to replace to.</param>
    public static IEnumerator<T> ReturnTmpRefForRepFunc(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        if (StaticWorker!.ReplacementObject == null)
            return Empty();
        if (StaticWorker!.ReplacementObject is IEnumerator<T> that && that != null)
            return that;
        return Empty();
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilFalse(Func{bool})"/>.
    /// </summary>
    public static IEnumerator<T> WaitUntilFalseHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, true, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilTrue(Func{bool})"/>.
    /// </summary>
    public static IEnumerator<T> WaitUntilTrueHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone(StaticWorker!.ReplacementObject as Func<bool>, false, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilZero{Number}(Func{Number})"/>.
    /// </summary>
    public static IEnumerator<T> WaitUntilZeroHelper<Number>(IEnumerator<T> coptr) where Number : INumber<Number>
    {
        StartIfNotExists();
        return StartWhenZeroDone<Number>(StaticWorker!.ReplacementObject as Func<Number>, Number.Zero, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="StartAfterCoroutine(CoroutineHandle)"/>.
    /// </summary>
    public static IEnumerator<T> StartAfterCoroutineHelper(IEnumerator<T> coptr)
    {
        StartIfNotExists();
        return StartWhenDone((CoroutineHandle?)StaticWorker!.ReplacementObject, coptr);
    }
}
#endif