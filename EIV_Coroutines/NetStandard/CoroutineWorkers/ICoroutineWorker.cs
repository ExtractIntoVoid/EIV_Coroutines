#if NETSTANDARD2_0
using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

public interface ICoroutineWorker
{
    /// <summary>
    /// Replacement object for able to work in update
    /// </summary>
    object? ReplacementObject { get; set; }
    /// <summary>
    /// Function to replace if Delay is set to NaN / infinity
    /// </summary>
    Func<IEnumerator<float>, IEnumerator<float>>? ReplacementFunction { get; set; }
    /// <summary>
    /// Should the updating pause
    /// </summary>
    bool PauseUpdate { get; set; }
    bool KillOnSuccess { get; set; }

    IEnumerable<Coroutine> Coroutines { get; }


    void Init();
    void Quit();
    void Update(float deltaTime);

    CoroutineHandle AddCoroutineInstance(Coroutine coroutine);
    void KillCoroutinesInstance(IList<CoroutineHandle> coroutines);
    void KillCoroutineInstance(CoroutineHandle coroutine);
    void KillCoroutineTagInstance(string tag);
    void PauseCoroutineInstance(CoroutineHandle coroutine);
    bool IsCoroutineExistsInstance(CoroutineHandle coroutine);
    bool IsCoroutineSuccessInstance(CoroutineHandle coroutine);
    bool IsCoroutineRunningInstance(CoroutineHandle coroutine);
    bool HasAnyCoroutinesInstance();
    Coroutine? GetCoroutine(CoroutineHandle coroutine);
}
#endif