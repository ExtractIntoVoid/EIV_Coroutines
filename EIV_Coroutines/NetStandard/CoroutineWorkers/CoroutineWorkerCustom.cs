#if NETSTANDARD2_0
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EIV_Coroutines.CoroutineWorkers;

/// <summary>
/// Custom worker.
/// </summary>
public class CoroutineWorkerCustom : ICoroutineWorker
{
    private readonly ConcurrentDictionary<CoroutineHandle, Coroutine> SafeCoroutines = [];

    /// <inheritdoc />
    public object? ReplacementObject { get; set; }

    /// <inheritdoc />
    public Func<IEnumerator<float>, IEnumerator<float>>? ReplacementFunction { get; set; }

    /// <inheritdoc />
    public bool PauseUpdate { get; set; } = false;

    /// <inheritdoc />
    public bool KillOnSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the current update rate.
    /// </summary>
    public static float UpdateRate { get; set; } = 1f;

    /// <inheritdoc />
    public IEnumerable<Coroutine> Coroutines
    {
        get
        {
            return SafeCoroutines.Values;
        }
    }

    #region Private fields
    private Thread? UpdateThread;
    private readonly Stopwatch Watch = new();
    private float prevTime = 0f;
    private float accumulator = 0f;
    #endregion

    #region Basic stuff (Init, Quit, Update)
    SynchronizationContext? mainContext;

    /// <inheritdoc />
    public void Init()
    {
        Watch.Start();
        prevTime = Watch.ElapsedMilliseconds / 1000;
        UpdateThread = new(ThreadUpdate)
        {
            IsBackground = true
        };
        UpdateThread.Start();
        mainContext = SynchronizationContext.Current;
    }

    /// <inheritdoc />
    public void Quit()
    {
        Kill();
        UpdateThread?.Interrupt();
        UpdateThread = null;
        Watch.Stop();
    }

    private void UpdateObj(object? obj)
    {
        UpdateDT((float)obj!);
    }

    /// <inheritdoc />
    public void UpdateDT(float deltaTime)
    {
        Kill();

        foreach (var coroutine in SafeCoroutines.ToList())
        {
            UpdateLogic(coroutine.Key, coroutine.Value, deltaTime);
        }

        Kill();
    }

    private void UpdateLogic(CoroutineHandle handle, Coroutine coroutine, float deltaTime)
    {
        // IF the delay for it is not zero, we remove from current delta.
        if (coroutine.Delay > 0f)
            coroutine.Delay -= deltaTime;

        // IF the delay is zero OR smaller (means we got negative time) we work on it.
        if (coroutine.Delay <= 0f)
        {
            Work(handle, coroutine);
        }

        // IF the delay is NaN we should use the replacementFunction for it!
        if (float.IsNaN(coroutine.Delay) && ReplacementFunction != null)
        {
            coroutine.Enumerator = ReplacementFunction(coroutine.Enumerator);
            Work(handle, coroutine);
            ReplacementFunction = null;
        }
    }

    private void Work(CoroutineHandle handle, Coroutine coroutine)
    {
        if (coroutine is { IsPaused: true } or { IsSuccess: true } or { ShouldKill: true })
            return;

        coroutine.IsRunning = true;

        if (coroutine.Enumerator.MoveNext())
        {
            coroutine.Delay = coroutine.Enumerator.Current;
        }
        else
        {
            coroutine.IsRunning = false;
            coroutine.IsSuccess = true;

            if (KillOnSuccess)
                coroutine.ShouldKill = true;
        }

        SafeCoroutines[handle] = coroutine;
    }

    #endregion
    #region Kills
    /// <inheritdoc />
    public void KillCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return;
        cor.ShouldKill = true;
        SafeCoroutines[coroutine] = cor;
    }

    /// <inheritdoc />
    public void KillCoroutinesInstance(IList<CoroutineHandle> coroutines)
    {
        foreach (CoroutineHandle coroutine in coroutines)
        {
            KillCoroutineInstance(coroutine);
        }
    }

    /// <inheritdoc />
    public void KillCoroutineTagInstance(string tag)
    {
        var cors = SafeCoroutines.Where(x => x.Value.Tag == tag).Select(x => x.Key).ToList();
        KillCoroutinesInstance(cors);
    }
    #endregion
    #region Checks
    /// <inheritdoc />
    public bool HasAnyCoroutinesInstance()
    {
        return !SafeCoroutines.IsEmpty;
    }

    /// <inheritdoc />
    public bool IsCoroutineExistsInstance(CoroutineHandle coroutine)
    {
        return SafeCoroutines.ContainsKey(coroutine);
    }

    /// <inheritdoc />
    public bool IsCoroutineSuccessInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return false;
        return cor.IsSuccess;
    }

    /// <inheritdoc />
    public bool IsCoroutineRunningInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return false;
        return cor.IsRunning;
    }
    #endregion
    #region Other Coroutine stuff
    /// <inheritdoc />
    public void PauseCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return;
        cor.IsPaused = !cor.IsPaused;
        SafeCoroutines[coroutine] = cor;
    }

    /// <inheritdoc />
    public CoroutineHandle AddCoroutineInstance(Coroutine coroutine)
    {
        coroutine.Delay = 0f;
        CoroutineHandle handle = CoroutineHandle.AsHandle(coroutine);
        SafeCoroutines.TryAdd(handle, coroutine);
        return handle;
    }

    /// <inheritdoc />
    public Coroutine? GetCoroutine(CoroutineHandle handle)
    {
        if (SafeCoroutines.TryGetValue(handle, out var coroutine))
            return coroutine;
        return null;
    }

    #endregion
    #region Private stuff
    private void Kill()
    {
        foreach (var coroutine in SafeCoroutines.ToList())
        {
            if (coroutine.Value.ShouldKill)
                SafeCoroutines.TryRemove(coroutine.Key, out _);
        }
    }

    private void ThreadUpdate()
    {
        if (UpdateThread == null)
            return;

        while (UpdateThread != null && UpdateThread.ThreadState == System.Threading.ThreadState.Background)
        {
            if (PauseUpdate)
                continue;
            float currTime = (float)(Watch.ElapsedMilliseconds / 1000d);
            accumulator += currTime - prevTime;
            prevTime = currTime;

            if (accumulator > UpdateRate)
            {
                accumulator -= UpdateRate;
                if (mainContext != null)
                    mainContext.Send(UpdateObj, UpdateRate);
                else
                    UpdateDT(UpdateRate);
            }
        }
    }
    #endregion
}
#endif