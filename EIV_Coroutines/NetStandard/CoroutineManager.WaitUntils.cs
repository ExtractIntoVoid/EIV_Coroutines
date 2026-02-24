#if NETSTANDARD2_0
using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager
{
    public static float WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
            return 0f;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilFalseHelper;
        return float.NaN;
    }

    public static float WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
            return 0f;

        StartIfNotExists();
        StaticWorker!.ReplacementObject = evaluatorFunc;
        StaticWorker!.ReplacementFunction = WaitUntilTrueHelper;
        return float.NaN;
    }

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