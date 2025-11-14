using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

public interface ICoroutineWorker<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
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
    /// Should the updating pause
    /// </summary>
    bool PauseUpdate { get; set; }
    bool KillOnSuccess { get; set; }

    IEnumerable<Coroutine<T>> Coroutines { get; }


    void Init();
    void Quit();
    void Update(T deltaTime);

    CoroutineHandle AddCoroutineInstance(Coroutine<T> coroutine);
    void KillCoroutinesInstance(IList<CoroutineHandle> coroutines);
    void KillCoroutineInstance(CoroutineHandle coroutine);
    void KillCoroutineTagInstance(string tag);
    void PauseCoroutineInstance(CoroutineHandle coroutine);
    bool IsCoroutineExistsInstance(CoroutineHandle coroutine);
    bool IsCoroutineSuccessInstance(CoroutineHandle coroutine);
    bool IsCoroutineRunningInstance(CoroutineHandle coroutine);
    bool HasAnyCoroutinesInstance();
    Coroutine<T>? GetCoroutine(CoroutineHandle coroutine);
}
