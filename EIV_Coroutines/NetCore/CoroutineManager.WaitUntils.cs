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
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilFalse<T>(Func<bool> evaluatorFunc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (evaluatorFunc == null || !evaluatorFunc())
        {
            return T.Zero;
        }

        StartIfNotExists<T>();
        GetWorker<T>()!.ReplacementObject = evaluatorFunc;
        GetWorker<T>()!.ReplacementFunction = WaitUntilFalseHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilTrue<T>(Func<bool> evaluatorFunc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (evaluatorFunc == null || evaluatorFunc())
        {
            return T.Zero;
        }

        StartIfNotExists<T>();
        GetWorker<T>()!.ReplacementObject = evaluatorFunc;
        GetWorker<T>()!.ReplacementFunction = WaitUntilTrueHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see cref="INumberBase{TSelf}.Zero"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilZero<T, TNumber>(Func<TNumber> evaluatorFunc)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
        where TNumber : INumber<TNumber>
    {
        if (evaluatorFunc == null)
        {
            return T.NaN;
        }

        if (evaluatorFunc() == TNumber.Zero)
        {
            return T.Zero;
        }

        StartIfNotExists<T>();
        GetWorker<T>()!.ReplacementObject = evaluatorFunc;
        GetWorker<T>()!.ReplacementFunction = WaitUntilZeroHelper<T, TNumber>;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T StartAfterCoroutine<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();

        if (GetWorker<T>()!.IsCoroutineSuccessInstance(coroutine) || !GetWorker<T>()!.IsCoroutineExistsInstance(coroutine))
        {
            return T.Zero;
        }

        GetWorker<T>()!.ReplacementObject = coroutine;
        GetWorker<T>()!.ReplacementFunction = StartAfterCoroutineHelper;
        return T.NaN;
    }
}
#endif