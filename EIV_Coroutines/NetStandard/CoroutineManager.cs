#if NETSTANDARD2_0
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

public partial class CoroutineManager
{
    public static ICoroutineWorker? StaticWorker { get; private set; }

    public static void StartIfNotExists()
    {
        if (StaticWorker == null)
            Start();
    }

    public static void Start(bool useDefault = true, Func<ICoroutineWorker>? func = null)
    {
        StaticWorker = useDefault switch
        {
            true => new CoroutineWorkerCustom(),
            _ => func?.Invoke(),
        };
        StaticWorker?.Init();
    }

    public static void Stop()
    {
        StaticWorker?.Quit();
        StaticWorker = null;
    }

    public static CoroutineHandle StartCoroutine(IEnumerator<float> objects, string tag = "")
    {
        StartIfNotExists();
        Coroutine coroutine = new(objects, tag);
        return StaticWorker!.AddCoroutineInstance(coroutine);
    }

    public static CoroutineHandle CallDelayed(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(DelayedCall(timeSpan, action), tag);
    }
    public static CoroutineHandle CallContinuously(Action action, string tag = "")
    {
        return StartCoroutine(CallContinuously(TimeSpan.Zero, action), tag);
    }
    public static CoroutineHandle CallPeriodically(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(CallContinuously(timeSpan, action), tag);
    }

    public static void KillCoroutines(IList<CoroutineHandle> coroutines)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutinesInstance(coroutines);
    }

    public static void KillCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutineInstance(coroutine);
    }
    public static void KillCoroutineTag(string tag)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutineTagInstance(tag);
    }

    public static void PauseCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        StaticWorker!.PauseCoroutineInstance(coroutine);
    }

    public static bool IsCoroutineExists(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineExistsInstance(coroutine);
    }

    public static bool IsCoroutineSuccess(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineSuccessInstance(coroutine);
    }

    public static bool IsCoroutineRunning(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineRunningInstance(coroutine);
    }

    public static bool HasAnyCoroutines()
    {
        StartIfNotExists();
        return StaticWorker!.HasAnyCoroutinesInstance();
    }
}

public class CoroutineFloatManager : CoroutineManager;
#endif