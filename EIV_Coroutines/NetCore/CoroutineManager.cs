#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class CoroutineManager<T> 
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Current and static worker.
    /// </summary>
    public static ICoroutineWorker<T>? StaticWorker { get; private set; }

    /// <summary>
    /// Starts the coroutine if not exists.
    /// </summary>
    public static void StartIfNotExists()
    {
        if (StaticWorker == null)
            Start();
    }

    /// <summary>
    /// Starts the worker.
    /// </summary>
    /// <param name="useDefault">Whenever should use the default (<see cref="CoroutineWorkerCustom{T}"/>) worker.</param>
    /// <param name="func">The new create function for the worker.</param>
    public static void Start(bool useDefault = true, Func<ICoroutineWorker<T>>? func = null)
    {
        StaticWorker = useDefault switch
        {
            true => new CoroutineWorkerCustom<T>(),
            _ => func?.Invoke(),
        };
        StaticWorker?.Init();
    }

    /// <summary>
    /// Stops and dispose the worker.
    /// </summary>
    public static void Stop()
    {
        StaticWorker?.Quit();
        StaticWorker = null;
    }

    /// <summary>
    /// Starts the coroutine.
    /// </summary>
    /// <param name="objects">The Coroutine.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle StartCoroutine(IEnumerator<T> objects, string tag = "")
    {
        StartIfNotExists();
        Coroutine<T> coroutine = new(objects, tag);
        return StaticWorker!.AddCoroutineInstance(coroutine);
    }

    /// <summary>
    /// Calls the action with a delayed <paramref name="timeSpan"/>.
    /// </summary>
    /// <param name="timeSpan">The delay time.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallDelayed(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(DelayedCall(timeSpan, action), tag);
    }

    /// <summary>
    /// Calls the action with a delayed <paramref name="delaySeconds"/>.
    /// </summary>
    /// <param name="delaySeconds">The delay time in seconds.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallDelayed(float delaySeconds, Action action, string tag = "")
    {
        return StartCoroutine(DelayedCall(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <summary>
    /// Calls the action every time.
    /// </summary>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallContinuously(Action action, string tag = "")
    {
        return StartCoroutine(CallContinuously(TimeSpan.Zero, action), tag);
    }

    /// <summary>
    /// Calls the action periodically with the <paramref name="timeSpan"/>.
    /// </summary>
    /// <param name="timeSpan">The period to call.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallPeriodically(TimeSpan timeSpan, Action action, string tag = "")
    {
        return StartCoroutine(CallContinuously(timeSpan, action), tag);
    }

    /// <summary>
    /// Calls the action periodically with the <paramref name="delaySeconds"/>.
    /// </summary>
    /// <param name="delaySeconds">The period to call in seconds.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallPeriodically(float delaySeconds, Action action, string tag = "")
    {
        return StartCoroutine(CallContinuously(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutinesInstance(IList{CoroutineHandle})"/>
    public static void KillCoroutines(IList<CoroutineHandle> coroutines)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutinesInstance(coroutines);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineInstance(CoroutineHandle)"/>
    public static void KillCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutineInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineTagInstance(string)"/>
    public static void KillCoroutineTag(string tag)
    {
        StartIfNotExists();
        StaticWorker!.KillCoroutineTagInstance(tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.PauseCoroutineInstance(CoroutineHandle)"/>
    public static void PauseCoroutine(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        StaticWorker!.PauseCoroutineInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineExistsInstance(CoroutineHandle)"/>
    public static bool IsCoroutineExists(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineExistsInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineSuccessInstance(CoroutineHandle)"/>
    public static bool IsCoroutineSuccess(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineSuccessInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineRunningInstance(CoroutineHandle)"/>
    public static bool IsCoroutineRunning(CoroutineHandle coroutine)
    {
        StartIfNotExists();
        return StaticWorker!.IsCoroutineRunningInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.HasAnyCoroutinesInstance()"/>
    public static bool HasAnyCoroutines()
    {
        StartIfNotExists();
        return StaticWorker!.HasAnyCoroutinesInstance();
    }
}

/// <summary>
/// The coroutine manager for <see cref="float"/>.
/// </summary>
public class CoroutineFloatManager : CoroutineManager<float>;

/// <summary>
/// The coroutine manager for <see cref="double"/>.
/// </summary>
public class CoroutineDoubleManager : CoroutineManager<double>;
#endif