using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines.Extensions;

public static class CoroutineStaticExt<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    public static T ReturnTimeAsT(long time)
    {
        if (typeof(T) == typeof(float))
            return T.CreateChecked((float)time);
        if (typeof(T) == typeof(double))
            return T.CreateChecked((double)time);
        return T.Zero;
    }

    public static T ReturnTimeAsT(double time)
    {
        if (typeof(T) == typeof(float))
            return T.CreateChecked((float)time);
        if (typeof(T) == typeof(double))
            return T.CreateChecked(time);
        return T.Zero;
    }

    public static ICoroutineWorker<T>? StaticWorker { get; private set; }

    public static void StartIfNotExists()
    {
        if (StaticWorker == null)
            Start();
    }

    public static void Start(byte worker = 0, Func<ICoroutineWorker<T>>? func = null)
    {
        switch (worker)
        {
            case 0:
                StaticWorker = new CoroutineWorkerCustom<T>();
                break;
            default:
                StaticWorker = func?.Invoke();
                break;
        }
        StaticWorker?.Init();
    }

    public static void Stop()
    {
        StaticWorker?.Quit();
        StaticWorker = null;
    }

    public static CoroutineHandle StartCoroutine(IEnumerator<T> objects, string tag = "")
    {
        StartIfNotExists();
        Coroutine<T> coroutine = new(objects, tag);
        StaticWorker!.AddCoroutineInstance(coroutine);
        return CoroutineHandle.AsHandle(coroutine);
    }

    public static CoroutineHandle CallDelayed(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(CoroutineEnumeratorExt<T>.DelayedCall(timeSpan, action), tag);
    }
    public static CoroutineHandle CallContinuously(Action action, string tag = "")
    {
        return StartCoroutine(CoroutineEnumeratorExt<T>.CallContinuously(TimeSpan.Zero, action), tag);
    }
    public static CoroutineHandle CallPeriodically(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(CoroutineEnumeratorExt<T>.CallContinuously(timeSpan, action), tag);
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
