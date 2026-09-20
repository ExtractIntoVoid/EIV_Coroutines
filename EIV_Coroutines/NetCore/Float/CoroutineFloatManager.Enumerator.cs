#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/> for <see cref="float"/>.
/// </summary>
public static partial class CoroutineFloatManager
{
    /// <summary>
    /// Creates a new empty coroutine.
    /// </summary>
    /// <returns>
    /// An empty coroutine.
    /// </returns>
    public static IEnumerator<float> Empty()
    {
        return CoroutineManager.Empty<float>();
    }

    /// <summary>
    /// A delayed call coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<float> DelayedCall(TimeSpan timeSpan, Action action)
    {
        return CoroutineManager.DelayedCall<float>(timeSpan, action);
    }

    /// <summary>
    /// A continous calling coroutine.
    /// </summary>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<float> CallContinuously(TimeSpan timeSpan, Action action)
    {
        return CoroutineManager.CallContinuously<float>(timeSpan, action);
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
    public static IEnumerator<float> StartWhenDone(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<float> pausedProc)
    {
        return CoroutineManager.StartWhenDone<float>(evaluatorFunc, continueOn, pausedProc);
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
    public static IEnumerator<float> StartWhenZeroDone<TNumber>(Func<TNumber>? evaluatorFunc, TNumber continueOn, IEnumerator<float> pausedProc)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.StartWhenZeroDone<float, TNumber>(evaluatorFunc, continueOn, pausedProc);
    }

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after the coroutine is finished.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="coroutine"/> to be success.
    /// </returns>
    public static IEnumerator<float> StartWhenDone(CoroutineHandle? coroutine, IEnumerator<float> pausedProc)
    {
        return CoroutineManager.StartWhenDone<float>(coroutine, pausedProc);
    }

    /// <summary>
    /// The replacement function to replace the current coroutine to <paramref name="coptr"/>.
    /// </summary>
    /// <param name="coptr">The coroutine to replace to.</param>
    /// <returns>
    /// The replacement coroutine or an empty coroutine.
    /// </returns>
    public static IEnumerator<float> ReturnTmpRefForRepFunc(IEnumerator<float> coptr)
    {
        return CoroutineManager.ReturnTmpRefForRepFunc<float>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilFalse(Func{bool})"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilFalse(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<float> WaitUntilFalseHelper(IEnumerator<float> coptr)
    {
        return CoroutineManager.WaitUntilFalseHelper<float>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilTrue(Func{bool})"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilTrue(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<float> WaitUntilTrueHelper(IEnumerator<float> coptr)
    {
        return CoroutineManager.WaitUntilTrueHelper<float>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilZero{TNumber}(Func{TNumber})"/>.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilZero{TNumber}(Func{TNumber})"/> coroutine.
    /// </returns>
    public static IEnumerator<float> WaitUntilZeroHelper<TNumber>(IEnumerator<float> coptr)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.WaitUntilZeroHelper<float, TNumber>(coptr);
    }

    /// <summary>
    /// Helper method for <see cref="StartAfterCoroutine(CoroutineHandle)"/>.
    /// </summary>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="StartAfterCoroutine(CoroutineHandle)"/> coroutine.
    /// </returns>
    public static IEnumerator<float> StartAfterCoroutineHelper(IEnumerator<float> coptr)
    {
        return CoroutineManager.StartAfterCoroutineHelper<float>(coptr);
    }
}
#endif