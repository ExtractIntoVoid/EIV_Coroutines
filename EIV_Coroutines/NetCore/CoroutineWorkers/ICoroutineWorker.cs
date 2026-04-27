#if NET5_0_OR_GREATER
using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

/// <summary>
/// A worker for a Coroutine.
/// </summary>
public interface ICoroutineWorker<T> 
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    /// <summary>
    /// Replacement object for able to work in update
    /// </summary>
    object? ReplacementObject { get; set; }

    /// <summary>
    /// Function to replace if Delay is set to NaN / infinity
    /// </summary>
    Func<IEnumerator<T>, IEnumerator<T>>? ReplacementFunction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether updates are paused.
    /// </summary>
    bool PauseUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the process should be terminated automatically when the operation
    /// completes successfully.
    /// </summary>
    bool KillOnSuccess { get; set; }

    /// <summary>
    /// Gets the collection of currently active coroutines managed by this instance.
    /// </summary>
    IEnumerable<Coroutine<T>> Coroutines { get; }

    /// <summary>
    /// Initializes the component and prepares it for use.
    /// </summary>
    void Init();

    /// <summary>
    /// Updates the object's state based on the elapsed time since the last update.
    /// </summary>
    /// <param name="deltaTime">The amount of time, in seconds, that has elapsed since the previous update. Must be non-negative.</param>
    void UpdateDT(T deltaTime);

    /// <summary>
    /// Terminates and stops the current worker.
    /// </summary>
    void Quit();

    /// <summary>
    /// Adds the coroutine to the instance.
    /// </summary>
    /// <param name="coroutine">The coroutine input.</param>
    /// <returns>The coroutine handle.</returns>
    CoroutineHandle AddCoroutineInstance(Coroutine<T> coroutine);

    /// <summary>
    /// Kills the <paramref name="coroutines"/> from the instance.
    /// </summary>
    /// <param name="coroutines">The list of coroutines to kill.</param>
    void KillCoroutinesInstance(IList<CoroutineHandle> coroutines);

    /// <summary>
    /// Kill the <paramref name="coroutine"/> from the instance.
    /// </summary>
    /// <param name="coroutine">The coroutine to kill.</param>
    void KillCoroutineInstance(CoroutineHandle coroutine);

    /// <summary>
    /// Kill the coroutines that has a tag as <paramref name="tag"/> from the instance.
    /// </summary>
    /// <param name="tag">The tag to kill with.</param>
    void KillCoroutineTagInstance(string tag);

    /// <summary>
    /// Pauses the <paramref name="coroutine"/> coroutine.
    /// </summary>
    /// <param name="coroutine">The handle to pause.</param>
    void PauseCoroutineInstance(CoroutineHandle coroutine);

    /// <summary>
    /// Checks whenever the <paramref name="coroutine"/> is exists.
    /// </summary>
    /// <param name="coroutine">The handle to check.</param>
    /// <returns><see langword="true"/> if exists, othewise. <see langword="false"/>.</returns>
    bool IsCoroutineExistsInstance(CoroutineHandle coroutine);

    /// <summary>
    /// Checks whenever the <paramref name="coroutine"/> is successly finished.
    /// </summary>
    /// <param name="coroutine">The handle to check.</param>
    /// <returns><see langword="true"/> if success, othewise. <see langword="false"/>.</returns>
    bool IsCoroutineSuccessInstance(CoroutineHandle coroutine);

    /// <summary>
    /// Checks whenever the <paramref name="coroutine"/> is running.
    /// </summary>
    /// <param name="coroutine">The handle to check.</param>
    /// <returns><see langword="true"/> if running, othewise. <see langword="false"/>.</returns>
    bool IsCoroutineRunningInstance(CoroutineHandle coroutine);

    /// <summary>
    /// Checks whenever has any coroutine stored.
    /// </summary>
    /// <returns><see langword="true"/> if has any, othewise. <see langword="false"/>.</returns>
    bool HasAnyCoroutinesInstance();

    /// <summary>
    /// Get the coroutine object from the handle.
    /// </summary>
    /// <param name="coroutine">The handle.</param>
    /// <returns>The result coroutine.</returns>
    Coroutine<T>? GetCoroutine(CoroutineHandle coroutine);
}
#endif