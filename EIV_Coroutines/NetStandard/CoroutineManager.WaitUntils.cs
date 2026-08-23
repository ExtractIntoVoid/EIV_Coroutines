#if NETSTANDARD2_0
using EIV_Coroutines.CoroutineWorkers;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker"/>.
/// </summary>
public partial class CoroutineManager
{
    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="false"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns>0 if success; otherwise NaN.</returns>
    public static float WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
        {
            return 0f;
        }

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilFalseHelper;
        return float.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="evaluatorFunc"/> returns <see langword="true"/>.
    /// </summary>
    /// <param name="evaluatorFunc">The function to check.</param>
    /// <returns>0 if success; otherwise NaN.</returns>
    public static float WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
        {
            return 0f;
        }

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilTrueHelper;
        return float.NaN;
    }

    /// <summary>
    /// Waits until <paramref name="coroutine"/> successfully run.
    /// </summary>
    /// <param name="coroutine">The coroutine to check.</param>
    /// <returns>0 if success; otherwise NaN.</returns>
    public static float StartAfterCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();

        if (StaticWorker!.IsCoroutineSuccessInstance(coroutine) || !StaticWorker!.IsCoroutineExistsInstance(coroutine))
        {
            return 0f;
        }

        StaticWorker!.ReplacementObject = coroutine;
        StaticWorker!.ReplacementFunction = StartAfterCoroutineHelper;
        return float.NaN;
    }
}
#endif