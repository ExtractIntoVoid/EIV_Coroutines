#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/>.
/// </summary>
/// <typeparam name="T">Any floating point type.</typeparam>
public partial class CoroutineManager<T>
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
        {
            return T.Zero;
        }

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilFalseHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
        {
            return T.Zero;
        }

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilTrueHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see cref="INumberBase{TSelf}.Zero"/>.
    /// </summary>
    /// <typeparam name="TNumber">Any number.</typeparam>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T WaitUntilZero<TNumber>(Func<TNumber> evaluatorFunc)
        where TNumber : INumber<TNumber>
    {
        if (evaluatorFunc() == TNumber.Zero)
        {
            return T.Zero;
        }

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilZeroHelper<TNumber>;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <returns><see cref="INumberBase{TSelf}.Zero"/> if success; otherwise <see cref="IFloatingPointIeee754{TSelf}.NaN"/>.</returns>
    public static T StartAfterCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();

        if (StaticWorker!.IsCoroutineSuccessInstance(coroutine) || !StaticWorker!.IsCoroutineExistsInstance(coroutine))
        {
            return T.Zero;
        }

        StaticWorker!.ReplacementObject = coroutine;
        StaticWorker!.ReplacementFunction = StartAfterCoroutineHelper;
        return T.NaN;
    }
}
#endif