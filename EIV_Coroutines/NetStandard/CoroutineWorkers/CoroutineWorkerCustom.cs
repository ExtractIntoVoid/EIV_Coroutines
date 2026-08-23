#if NETSTANDARD2_0
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EIV_Coroutines.CoroutineWorkers;

/// <summary>
/// A custom worker.
/// </summary>
public class CoroutineWorkerCustom : ICoroutineWorker
{
    /// <summary>
    /// Gets or sets the current update rate.
    /// </summary>
    public static float UpdateRate { get; set; } = 1;

    private Thread? updateThread;
    private readonly Stopwatch watch = new();
    private float prevTime = 0;
    private float accumulator = 0;
    private SynchronizationContext? mainContext;
    private readonly ConcurrentDictionary<CoroutineHandle, Coroutine> safeCoroutines = [];

    /// <inheritdoc />
    public event Action<CoroutineHandle, Exception>? OnException;

    /// <inheritdoc />
    public object? ReplacementObject { get; set; }

    /// <inheritdoc />
    public Func<IEnumerator<float>, IEnumerator<float>>? ReplacementFunction { get; set; }

    /// <inheritdoc />
    public bool PauseUpdate { get; set; } = false;

    /// <inheritdoc />
    public bool KillOnSuccess { get; set; } = true;

    /// <inheritdoc />
    public IEnumerable<Coroutine> Coroutines
    {
        get
        {
            return safeCoroutines.Values;
        }
    }

    /// <inheritdoc />
    public void Init()
    {
        safeCoroutines.Clear();
        watch.Start();
        prevTime = watch.ElapsedMilliseconds / 1000;
        updateThread = new(ThreadUpdate)
        {
            IsBackground = true,
        };
        updateThread.Start();
        mainContext = SynchronizationContext.Current;
    }

    /// <inheritdoc />
    public void Quit()
    {
        safeCoroutines.Clear();
        updateThread?.Interrupt();
        updateThread = null;
        watch.Stop();
    }

    /// <inheritdoc />
    public void UpdateDT(float deltaTime)
    {
        Kill();

        foreach (var coroutine in safeCoroutines.ToList())
        {
            UpdateLogic(coroutine.Key, coroutine.Value, deltaTime);
        }

        Kill();
    }

    /// <inheritdoc />
    public void KillCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
        {
            return;
        }

        cor.ShouldKill = true;
        safeCoroutines[coroutine] = cor;
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
        var cors = safeCoroutines.Where(x => x.Value.Tag == tag).Select(x => x.Key).ToList();
        KillCoroutinesInstance(cors);
    }

    /// <inheritdoc />
    public bool HasAnyCoroutinesInstance()
    {
        return !safeCoroutines.IsEmpty;
    }

    /// <inheritdoc />
    public bool IsCoroutineExistsInstance(CoroutineHandle coroutine)
    {
        return safeCoroutines.ContainsKey(coroutine);
    }

    /// <inheritdoc />
    public bool IsCoroutineSuccessInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
        {
            return false;
        }

        return cor.IsSuccess;
    }

    /// <inheritdoc />
    public bool IsCoroutinePausedInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
        {
            return false;
        }

        return cor.IsPaused;
    }

    /// <inheritdoc />
    public bool IsCoroutineRunningInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
        {
            return false;
        }

        return cor.IsRunning;
    }

    /// <inheritdoc />
    public void PauseCoroutineInstance(CoroutineHandle coroutine)
    {
        var cor = GetCoroutine(coroutine);
        if (cor is null)
        {
            return;
        }

        cor.IsPaused = !cor.IsPaused;
        safeCoroutines[coroutine] = cor;
    }

    /// <inheritdoc />
    public CoroutineHandle AddCoroutineInstance(Coroutine coroutine)
    {
        coroutine.Delay = 0;
        CoroutineHandle handle = CoroutineHandle.AsHandle(coroutine);
        safeCoroutines.TryAdd(handle, coroutine);
        return handle;
    }

    /// <inheritdoc />
    public Coroutine? GetCoroutine(CoroutineHandle handle)
    {
        if (safeCoroutines.TryGetValue(handle, out var coroutine))
        {
            return coroutine;
        }

        return null;
    }

    private void UpdateObj(object? obj)
    {
        UpdateDT((float)obj!);
    }

    private void UpdateLogic(CoroutineHandle handle, Coroutine coroutine, float deltaTime)
    {
        // IF the delay for it is not zero, we remove from current delta.
        if (coroutine.Delay > 0)
        {
            coroutine.Delay -= deltaTime;
        }

        // IF the delay is zero OR smaller (means we got negative time) we work on it.
        if (coroutine.Delay <= 0 || coroutine.Delay == float.NegativeInfinity)
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
        // We should not touch paused and to be killed ones.
        if (coroutine is { IsPaused: true } or { ShouldKill: true })
        {
            return;
        }

        // mark to be killed when success.
        if (KillOnSuccess && coroutine is { IsSuccess: true })
        {
            coroutine.ShouldKill = true;
            safeCoroutines[handle] = coroutine;
            return;
        }

        coroutine.IsRunning = true;

        try
        {
            if (!coroutine.Enumerator.MoveNext())
            {
                coroutine.IsRunning = false;
                coroutine.IsSuccess = true;
            }
            else
            {
                coroutine.Delay = coroutine.Enumerator.Current;
            }

            safeCoroutines[handle] = coroutine;
        }
        catch (Exception ex)
        {
            OnException?.Invoke(handle, ex);
        }
    }

    private void Kill()
    {
        foreach (var coroutine in safeCoroutines.ToList())
        {
            if (coroutine.Value.ShouldKill)
            {
                safeCoroutines.TryRemove(coroutine.Key, out _);
            }
        }
    }

    private void ThreadUpdate()
    {
        if (updateThread == null)
        {
            return;
        }

        while (updateThread != null && updateThread.ThreadState == System.Threading.ThreadState.Background)
        {
            if (PauseUpdate)
            {
                continue;
            }

            float currTime = (float)(watch.ElapsedMilliseconds / 1000d);
            accumulator += currTime - prevTime;
            prevTime = currTime;

            if (accumulator > UpdateRate)
            {
                accumulator -= UpdateRate;
                if (mainContext != null)
                {
                    mainContext.Send(UpdateObj, UpdateRate);
                }
                else
                {
                    UpdateDT(UpdateRate);
                }
            }
        }
    }
}
#endif