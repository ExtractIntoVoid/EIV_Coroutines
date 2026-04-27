#if NET5_0_OR_GREATER
using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager<T> 
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    public static T WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
            return T.Zero;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilFalseHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    public static T WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
            return T.Zero;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilTrueHelper;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see cref="INumberBase{TSelf}.Zero"/>.
    /// </summary>
    public static T WaitUntilZero<Number>(Func<Number> evaluatorFunc) where Number : INumber<Number>
    {
        if (evaluatorFunc() == Number.Zero)
            return T.Zero;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilZeroHelper<Number>;
        return T.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    public static T StartAfterCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();

        if (StaticWorker!.IsCoroutineSuccessInstance(coroutine))
            return T.Zero;

        StaticWorker!.ReplacementObject = coroutine;
        StaticWorker!.ReplacementFunction = StartAfterCoroutineHelper;
        return T.Zero;
    }
}
#endif