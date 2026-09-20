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
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static double WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        return CoroutineManager.WaitUntilFalse<double>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static double WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        return CoroutineManager.WaitUntilTrue<double>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see cref="INumberBase{TSelf}.Zero"/>.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static double WaitUntilZero<TNumber>(Func<TNumber> evaluatorFunc)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.WaitUntilZero<double, TNumber>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static double StartAfterCoroutine(CoroutineHandle coroutine)
    {
        return CoroutineManager.StartAfterCoroutine<double>(coroutine);
    }
}
#endif