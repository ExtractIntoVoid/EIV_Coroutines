using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

public class CoroutineWorkerCustom<T> : ICoroutineWorker<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    private readonly ConcurrentDictionary<CoroutineHandle, Coroutine<T>> SafeCoroutines = [];
    public object? ReplacementObject { get; set; }
    public Func<IEnumerator<T>, IEnumerator<T>>? ReplacementFunction { get; set; }
    public bool PauseUpdate { get; set; } = false;
    public bool KillOnSuccess { get; set; } = true;
    public static T UpdateRate { get; set; } = T.One;

    public IEnumerable<Coroutine<T>> Coroutines
    {
        get
        {
            return SafeCoroutines.Values;
        }
    }

    #region Private fields
    private Thread? UpdateThread;
    private readonly Stopwatch Watch = new();
    private T prevTime = T.Zero;
    private T accumulator = T.Zero;
    #endregion

    #region Basic stuff (Init, Quit, Update)
    ExecutionContext? mainContext;
    public void Init()
    {
        Watch.Start();
        prevTime = CoroutineManager<T>.ReturnTimeAsT(Watch.ElapsedMilliseconds / 1000);
        UpdateThread = new(ThreadUpdate)
        {
            IsBackground = true
        };
        UpdateThread.Start();
        mainContext = ExecutionContext.Capture();
    }

    public void Quit()
    {
        Kill();
        UpdateThread?.Interrupt();
        UpdateThread = null;
        Watch.Stop();
    }

    private void UpdateObj(object? obj)
    {
        Update((T)obj!);
    }

    public void Update(T deltaTime)
    {
        Kill();

        foreach (var coroutine in SafeCoroutines.ToList())
        {
            UpdateLogic(coroutine.Key, coroutine.Value, deltaTime);
        }

        Kill();
    }

    private void UpdateLogic(CoroutineHandle handle, Coroutine<T> coroutine, T deltaTime)
    {
        // IF the delay for it is not zero, we remove from current delta.
        if (coroutine.Delay > T.Zero)
            coroutine.Delay -= deltaTime;

        // IF the delay is zero OR smaller (means we got negative time) we work on it.
        if (coroutine.Delay <= T.Zero)
        {
            Work(handle, coroutine);
        }

        // IF the delay is NaN we should use the replacementFunction for it!
        if (T.IsNaN(coroutine.Delay) && ReplacementFunction != null)
        {
            coroutine.Enumerator = ReplacementFunction(coroutine.Enumerator);
            Work(handle, coroutine);
            ReplacementFunction = null;
        }
    }

    private void Work(CoroutineHandle handle, Coroutine<T> coroutine)
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
    public void KillCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return;
        cor.ShouldKill = true;
        SafeCoroutines[coroutine] = cor;
    }

    public void KillCoroutinesInstance(IList<CoroutineHandle> coroutines)
    {
        foreach (CoroutineHandle coroutine in coroutines)
        {
            KillCoroutineInstance(coroutine);
        }
    }

    public void KillCoroutineTagInstance(string tag)
    {
        var cors = SafeCoroutines.Where(x => x.Value.Tag == tag).Select(x => x.Key).ToList();
        KillCoroutinesInstance(cors);
    }
    #endregion
    #region Checks
    public bool HasAnyCoroutinesInstance()
    {
        return !SafeCoroutines.IsEmpty;
    }

    public bool IsCoroutineExistsInstance(CoroutineHandle coroutine)
    {
        return SafeCoroutines.ContainsKey(coroutine);
    }

    public bool IsCoroutineSuccessInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return false;
        return cor.IsSuccess;
    }
    public bool IsCoroutineRunningInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return false;
        return cor.IsRunning;
    }
    #endregion
    #region Other Coroutine stuff
    public void PauseCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
            return;
        cor.IsPaused = !cor.IsPaused;
        SafeCoroutines[coroutine] = cor;
    }

    public CoroutineHandle AddCoroutineInstance(Coroutine<T> coroutine)
    {
        coroutine.Delay = T.Zero;
        CoroutineHandle handle = CoroutineHandle.AsHandle(coroutine);
        SafeCoroutines.TryAdd(handle, coroutine);
        return handle;
    }

    public Coroutine<T>? GetCoroutine(CoroutineHandle handle)
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
            T currTime = CoroutineManager<T>.ReturnTimeAsT(Watch.ElapsedMilliseconds / 1000d);
            accumulator += currTime - prevTime;
            prevTime = currTime;

            if (accumulator > UpdateRate)
            {
                accumulator -= UpdateRate;
                if (mainContext != null)
                    ExecutionContext.Run(mainContext, UpdateObj, UpdateRate);
                else
                    Update(UpdateRate);
            }
        }
    }
    #endregion
}
