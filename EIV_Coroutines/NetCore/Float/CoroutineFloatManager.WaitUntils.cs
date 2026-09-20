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
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static float WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        return CoroutineManager.WaitUntilFalse<float>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static float WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        return CoroutineManager.WaitUntilTrue<float>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see cref="INumberBase{TSelf}.Zero"/>.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static float WaitUntilZero<TNumber>(Func<TNumber> evaluatorFunc)
        where TNumber : INumber<TNumber>
    {
        return CoroutineManager.WaitUntilZero<float, TNumber>(evaluatorFunc);
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static float StartAfterCoroutine(CoroutineHandle coroutine)
    {
        return CoroutineManager.StartAfterCoroutine<float>(coroutine);
    }
}
#endif