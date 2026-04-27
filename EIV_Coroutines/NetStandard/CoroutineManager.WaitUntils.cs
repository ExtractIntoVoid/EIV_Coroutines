#if NETSTANDARD2_0
namespace EIV_Coroutines;

public partial class CoroutineManager
{
    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    public static float WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
            return 0f;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilFalseHelper;
        return float.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    public static float WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
            return 0f;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilTrueHelper;
        return float.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    public static float StartAfterCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();

        if (StaticWorker!.IsCoroutineSuccessInstance(coroutine))
            return 0f;

        StaticWorker!.ReplacementObject = coroutine;
        StaticWorker!.ReplacementFunction = StartAfterCoroutineHelper;
        return 0f;
    }
}
#endif