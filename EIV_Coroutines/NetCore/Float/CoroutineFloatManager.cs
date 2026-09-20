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
    /// Gets the current worker.
    /// </summary>
    /// <returns>The worker or <see langword="null"/>.</returns>
    public static ICoroutineWorker<float>? GetWorker()
    {
        return CoroutineManager.GetWorker<float>();
    }

    /// <summary>
    /// Starts the coroutine if not exists.
    /// </summary>
    public static void StartIfNotExists()
    {
        CoroutineManager.StartIfNotExists<float>();
    }

    /// <summary>
    /// Starts the worker.
    /// </summary>
    /// <param name="useDefault">Whenever should use the default (<see cref="CoroutineWorkerCustom{T}"/>) worker.</param>
    /// <param name="func">The new create function for the worker.</param>
    public static void Start(bool useDefault = true, Func<ICoroutineWorker<float>>? func = null)
    {
        CoroutineManager.Start<float>(useDefault, func);
    }

    /// <summary>
    /// Stops and dispose the worker.
    /// </summary>
    public static void Stop()
    {
        CoroutineManager.Stop<float>();
    }

    /// <summary>
    /// Starts the coroutine.
    /// </summary>
    /// <param name="objects">The Coroutine.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle StartCoroutine(IEnumerator<float> objects, string tag = "")
    {
        return CoroutineManager.StartCoroutine<float>(objects, tag);
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
        return CoroutineManager.StartCoroutine<float>(DelayedCall(timeSpan, action), tag);
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
        return CoroutineManager.StartCoroutine<float>(DelayedCall(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <summary>
    /// Calls the action every time.
    /// </summary>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallContinuously(Action action, string tag = "")
    {
        return CoroutineManager.StartCoroutine<float>(CallContinuously(TimeSpan.Zero, action), tag);
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
        return CoroutineManager.StartCoroutine<float>(CallContinuously(timeSpan, action), tag);
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
        return CoroutineManager.StartCoroutine<float>(CallContinuously(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutinesInstance(IList{CoroutineHandle})"/>
    public static void KillCoroutines(IList<CoroutineHandle> coroutines)
    {
        CoroutineManager.KillCoroutines<float>(coroutines);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineInstance(CoroutineHandle)"/>
    public static void KillCoroutine(CoroutineHandle coroutine)
    {
        CoroutineManager.KillCoroutine<float>(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineTagInstance(string)"/>
    public static void KillCoroutineTag<T>(string tag)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        CoroutineManager.KillCoroutineTag<float>(tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.PauseCoroutineInstance(CoroutineHandle)"/>
    public static void PauseCoroutine(CoroutineHandle coroutine)
    {
        CoroutineManager.PauseCoroutine<float>(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineExistsInstance(CoroutineHandle)"/>
    public static bool IsCoroutineExists<T>(CoroutineHandle coroutine)
    {
        return CoroutineManager.IsCoroutineExists<float>(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineSuccessInstance(CoroutineHandle)"/>
    public static bool IsCoroutineSuccess(CoroutineHandle coroutine)
    {
        return CoroutineManager.IsCoroutineSuccess<float>(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineRunningInstance(CoroutineHandle)"/>
    public static bool IsCoroutineRunning(CoroutineHandle coroutine)
    {
        return CoroutineManager.IsCoroutineRunning<float>(coroutine);
    }

        /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutinePausedInstance(CoroutineHandle)"/>
    public static bool IsCoroutinePaused(CoroutineHandle coroutine)
    {
        return CoroutineManager.IsCoroutinePaused<float>(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.HasAnyCoroutinesInstance()"/>
    public static bool HasAnyCoroutines()
    {
        return CoroutineManager.HasAnyCoroutines<float>();
    }
}
#endif