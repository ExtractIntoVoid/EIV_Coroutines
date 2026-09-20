#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Numerics;

namespace EIV_Coroutines;

/// <summary>
/// Manages coroutine with <see cref="ICoroutineWorker{T}"/>.
/// </summary>
public static partial class CoroutineManager
{
    private static object? staticWorker;

    /// <summary>
    /// Gets the current worker.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <returns>The worker or <see langword="null"/>.</returns>
    public static ICoroutineWorker<T>? GetWorker<T>()
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (staticWorker == null)
        {
            return null;
        }

        return (ICoroutineWorker<T>?)staticWorker;
    }

    /// <summary>
    /// Starts the coroutine if not exists.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    public static void StartIfNotExists<T>()
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        if (staticWorker == null)
        {
            Start<T>();
        }
    }

    /// <summary>
    /// Starts the worker.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="useDefault">Whenever should use the default (<see cref="CoroutineWorkerCustom{T}"/>) worker.</param>
    /// <param name="func">The new create function for the worker.</param>
    public static void Start<T>(bool useDefault = true, Func<ICoroutineWorker<T>>? func = null)
            where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        staticWorker = useDefault switch
        {
            true => new CoroutineWorkerCustom<T>(),
            _ => func?.Invoke(),
        };
        GetWorker<T>()?.Init();
    }

    /// <summary>
    /// Stops and dispose the worker.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    public static void Stop<T>()
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        GetWorker<T>()?.Quit();
        staticWorker = null;
    }

    /// <summary>
    /// Starts the coroutine.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="objects">The Coroutine.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle StartCoroutine<T>(IEnumerator<T> objects, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        Coroutine<T> coroutine = new(objects, tag);
        return GetWorker<T>()!.AddCoroutineInstance(coroutine);
    }

    /// <summary>
    /// Calls the action with a delayed <paramref name="timeSpan"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="timeSpan">The delay time.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallDelayed<T>(TimeSpan timeSpan, Action action, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return StartCoroutine(DelayedCall<T>(timeSpan, action), tag);
    }

    /// <summary>
    /// Calls the action with a delayed <paramref name="delaySeconds"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="delaySeconds">The delay time in seconds.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallDelayed<T>(float delaySeconds, Action action, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return StartCoroutine(DelayedCall<T>(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <summary>
    /// Calls the action every time.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallContinuously<T>(Action action, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return StartCoroutine(CallContinuously<T>(TimeSpan.Zero, action), tag);
    }

    /// <summary>
    /// Calls the action periodically with the <paramref name="timeSpan"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="timeSpan">The period to call.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallPeriodically<T>(TimeSpan timeSpan, Action action, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return StartCoroutine(CallContinuously<T>(timeSpan, action), tag);
    }

    /// <summary>
    /// Calls the action periodically with the <paramref name="delaySeconds"/>.
    /// </summary>
    /// <typeparam name="T">Any floating point type.</typeparam>
    /// <param name="delaySeconds">The period to call in seconds.</param>
    /// <param name="action">The action to call.</param>
    /// <param name="tag">The tag associated with the coroutine.</param>
    /// <returns>The handle of the new coroutine.</returns>
    public static CoroutineHandle CallPeriodically<T>(float delaySeconds, Action action, string tag = "")
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        return StartCoroutine(CallContinuously<T>(TimeSpan.FromSeconds(delaySeconds), action), tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutinesInstance(IList{CoroutineHandle})"/>
    public static void KillCoroutines<T>(IList<CoroutineHandle> coroutines)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        GetWorker<T>()!.KillCoroutinesInstance(coroutines);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineInstance(CoroutineHandle)"/>
    public static void KillCoroutine<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        GetWorker<T>()!.KillCoroutineInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.KillCoroutineTagInstance(string)"/>
    public static void KillCoroutineTag<T>(string tag)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        GetWorker<T>()!.KillCoroutineTagInstance(tag);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.PauseCoroutineInstance(CoroutineHandle)"/>
    public static void PauseCoroutine<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        GetWorker<T>()!.PauseCoroutineInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineExistsInstance(CoroutineHandle)"/>
    public static bool IsCoroutineExists<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return GetWorker<T>()!.IsCoroutineExistsInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineSuccessInstance(CoroutineHandle)"/>
    public static bool IsCoroutineSuccess<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return GetWorker<T>()!.IsCoroutineSuccessInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutineRunningInstance(CoroutineHandle)"/>
    public static bool IsCoroutineRunning<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return GetWorker<T>()!.IsCoroutineRunningInstance(coroutine);
    }

        /// <inheritdoc cref="ICoroutineWorker{T}.IsCoroutinePausedInstance(CoroutineHandle)"/>
    public static bool IsCoroutinePaused<T>(CoroutineHandle coroutine)
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return GetWorker<T>()!.IsCoroutinePausedInstance(coroutine);
    }

    /// <inheritdoc cref="ICoroutineWorker{T}.HasAnyCoroutinesInstance()"/>
    public static bool HasAnyCoroutines<T>()
        where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
    {
        StartIfNotExists<T>();
        return GetWorker<T>()!.HasAnyCoroutinesInstance();
    }
}
#endif