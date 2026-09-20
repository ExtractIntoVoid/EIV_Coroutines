#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/>.
/// </summary>
public static partial class CoroutineManager
{
    /// <summary>
    /// Creates a new empty coroutine.
    /// </summary>
   /// <typeparam name="T">Any floating point type.</typeparam>
    /// <returns>
    /// An empty coroutine.
    /// </returns>
    public static IEnumerator<T> Empty<T>()
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        yield return T.Zero;
    }

    /// <summary>
    /// A delayed call coroutine.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<T> DelayedCall<T>(TimeSpan timeSpan, Action action)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        yield return T.CreateChecked(timeSpan.TotalSeconds);
        action?.Invoke();
    }

    /// <summary>
    /// A continous calling coroutine.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="timeSpan">The seconds to wait until <paramref name="action"/> run.</param>
    /// <param name="action">The action to run after <paramref name="timeSpan"/>.</param>
    /// <returns>
    /// A coroutine that waits <paramref name="timeSpan"/> seconds then call <paramref name="action"/>.
    /// </returns>
    public static IEnumerator<T> CallContinuously<T>(TimeSpan timeSpan, Action action)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        while (true)
        {
            yield return T.CreateChecked(timeSpan.TotalSeconds);
            action?.Invoke();
        }
    }

    /// <summary>
    /// A helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="evaluatorFunc"/> to be <paramref name="continueOn"/>.
    /// </returns>
    public static IEnumerator<T> StartWhenDone<T>(Func<bool>? evaluatorFunc, bool continueOn, IEnumerator<T> pausedProc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (evaluatorFunc == null)
        {
            yield break;
        }

        while (evaluatorFunc() == continueOn)
        {
            yield return T.NegativeInfinity;
        }

        StartIfNotExists<T>();
        GetWorker<T>()!.ReplacementObject = pausedProc;
        GetWorker<T>()!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after allowed contition.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <param name="continueOn">The condition it should continue the paused process.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="evaluatorFunc"/> to be <paramref name="continueOn"/>.
    /// </returns>
    public static IEnumerator<T> StartWhenZeroDone<T, TNumber>(Func<TNumber>? evaluatorFunc, TNumber continueOn, IEnumerator<T> pausedProc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
        where TNumber : INumber<TNumber>
    {
        if (evaluatorFunc == null)
        {
            yield break;
        }

        while (evaluatorFunc() != continueOn)
        {
            yield return T.NegativeInfinity;
        }

        StartIfNotExists<T>();
        GetWorker<T>()!.ReplacementObject = pausedProc;
        GetWorker<T>()!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    /// <summary>
    /// Helper function that starts the <paramref name="pausedProc"/> after the coroutine is finished.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <param name="pausedProc">The paused process.</param>
    /// <returns>
    /// A coroutine that waits for <paramref name="coroutine"/> to be success.
    /// </returns>
    public static IEnumerator<T> StartWhenDone<T>(CoroutineHandle? coroutine, IEnumerator<T> pausedProc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (!coroutine.HasValue)
        {
            yield break;
        }

        StartIfNotExists<T>();

        // Return to original if no coroutine found.
        if (!GetWorker<T>()!.IsCoroutineExistsInstance(coroutine.Value))
        {
            GetWorker<T>()!.ReplacementObject = pausedProc;
            GetWorker<T>()!.ReplacementFunction = ReturnTmpRefForRepFunc;
            yield return T.NaN;
        }

        while (!GetWorker<T>()!.IsCoroutineSuccessInstance(coroutine.Value))
        {
            yield return T.NegativeInfinity;
        }

        GetWorker<T>()!.ReplacementObject = pausedProc;
        GetWorker<T>()!.ReplacementFunction = ReturnTmpRefForRepFunc;
        yield return T.NaN;
    }

    /// <summary>
    /// The replacement function to replace the current coroutine to <paramref name="coptr"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coptr">The coroutine to replace to.</param>
    /// <returns>
    /// The replacement coroutine or an empty coroutine.
    /// </returns>
    public static IEnumerator<T> ReturnTmpRefForRepFunc<T>(IEnumerator<T> coptr)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        if (GetWorker<T>()!.ReplacementObject == null)
        {
            return Empty<T>();
        }

        if (GetWorker<T>()!.ReplacementObject is IEnumerator<T> that)
        {
            return that;
        }

        return Empty<T>();
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilFalse(Func{bool})"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilFalse(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<T> WaitUntilFalseHelper<T>(IEnumerator<T> coptr)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return StartWhenDone(GetWorker<T>()!.ReplacementObject as Func<bool>, true, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilTrue(Func{bool})"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilTrue(Func{bool})"/> coroutine.
    /// </returns>
    public static IEnumerator<T> WaitUntilTrueHelper<T>(IEnumerator<T> coptr)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return StartWhenDone(GetWorker<T>()!.ReplacementObject as Func<bool>, false, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="WaitUntilZero{T, Number}(Func{Number})"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="WaitUntilZero{T, Number}(Func{Number})"/> coroutine.
    /// </returns>
    public static IEnumerator<T> WaitUntilZeroHelper<T, TNumber>(IEnumerator<T> coptr)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
        where TNumber : INumber<TNumber>
    {
        StartIfNotExists<T>();
        return StartWhenZeroDone<T, TNumber>(GetWorker<T>()!.ReplacementObject as Func<TNumber>, TNumber.Zero, coptr);
    }

    /// <summary>
    /// Helper method for <see cref="StartAfterCoroutine(CoroutineHandle)"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coptr">The original coroutine.</param>
    /// <returns>
    /// The new <see cref="StartAfterCoroutine(CoroutineHandle)"/> coroutine.
    /// </returns>
    public static IEnumerator<T> StartAfterCoroutineHelper<T>(IEnumerator<T> coptr)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return StartWhenDone((CoroutineHandle?)GetWorker<T>()!.ReplacementObject, coptr);
    }
}
#endif