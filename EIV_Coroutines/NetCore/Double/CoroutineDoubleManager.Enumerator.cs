#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/> for <see cref="double"/>.
/// </summary>
public static partial class CoroutineDoubleManager
{
    /// <summary>
    /// Creates a new empty coroutine.
    /// </summary>
    /// <returns>
    /// An empty coroutine.
    /// </returns>
    public static IEnumerator<double> Empty()
    {
        return CoroutineManager.Empty<double>();
    }

    /// <summary>
    /// A delayed call coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<double> DelayedCall(TimeSpan timeSpan, Action action)
    {
        return CoroutineManager.DelayedCall<double>(timeSpan, action);
    }

    /// <summary>
    /// A continous calling coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<double> CallContinuously(TimeSpan timeSpan, Action action)
    {
        return CoroutineManager.CallContinuously<double>(timeSpan, action);
    }

    /// <summary>
    /// A helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="evaluatorFunc"/> to be <paramref name="continueOn"/>.
    /// </returns>
    public static IEnumerator<double> StartWhenDone(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<double> pausedProc)
    {
        return CoroutineManager.StartWhenDone<double>(evaluatorFunc, continueOn, pausedProc);
    }

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="evaluatorFunc"/> to be <paramref name="continueOn"/>.
    /// </returns>
    public static IEnumerator<double> StartWhenZeroDone<TNumber>(Func<TNumber>? evaluatorFunc, TNumber continueOn, IEnumerator<double> pausedProc)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.StartWhenZeroDone<double, TNumber>(evaluatorFunc, continueOn, pausedProc);
    }

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after the coroutine is finished.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="coroutine"/> to be success.
    /// </returns>
    public static IEnumerator<double> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<double> pausedProc)
    {
        return CoroutineManager.StartWhenDone<double>(coroutine, pausedProc);
    }

    /// <summary>
    /// The replacement function to replace the current coroutine to <paramref name="coptr"/>.
    /// </summary>
    /// <param name="coptr">The coroutine to replace to.</param>
    /// <returns>
    /// The replacement coroutine or an empty coroutine.
    /// </returns>
    public static IEnumerator<double> ReturnTmpRefForRepFunc(IEnumerator<double> coptr)
    {
        return CoroutineManager.ReturnTmpRefForRepFunc<double>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilFalse(Func{bool})"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilFalse(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<double> WaitUntilFalseHelper(IEnumerator<double> coptr)
    {
        return CoroutineManager.WaitUntilFalseHelper<double>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilTrue(Func{bool})"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilTrue(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<double> WaitUntilTrueHelper(IEnumerator<double> coptr)
    {
        return CoroutineManager.WaitUntilTrueHelper<double>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilZero{TNumber}(Func{TNumber})"/>.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilZero{TNumber}(Func{TNumber})"/> coroutine.
    /// </returns>
    public static IEnumerator<double> WaitUntilZeroHelper<TNumber>(IEnumerator<double> coptr)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.WaitUntilZeroHelper<double, TNumber>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="StartAfterCoroutine(CoroutineHandle)"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="StartAfterCoroutine(CoroutineHandle)"/> coroutine.
    /// </returns>
    public static IEnumerator<double> StartAfterCoroutineHelper(IEnumerator<double> coptr)
    {
        return CoroutineManager.StartAfterCoroutineHelper<double>(coptr);
    }
}
#endif