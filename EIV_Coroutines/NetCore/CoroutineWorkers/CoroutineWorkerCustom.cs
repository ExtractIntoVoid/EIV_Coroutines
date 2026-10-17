#if NET5_0_OR_GREATER
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

/// <summary>
/// A custom worker.
/// </summary>
/// <typeparam name="T">Any floating point type.</typeparam>
public class CoroutineWorkerCustom<T> : ICoroutineWorker<T>
    where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    private static readonly T OneMillisecond = T.CreateChecked(1000);
    private volatile bool isRunning;
    private Thread? updateThread;
    private readonly Stopwatch watch = new();
    private SynchronizationContext? mainContext;
    private readonly ConcurrentDictionary<CoroutineHandle, Coroutine<T>> safeCoroutines = [];

    /// <inheritdoc />
    public event EventHandler<CoroutineExceptionEventArgs>? OnException;

    /// <summary>
    /// Gets or sets the current update rate.
    /// </summary>
    public T UpdateRate { get; set; } = T.One;

    /// <inheritdoc />
    public object? ReplacementObject { get; set; }

    /// <inheritdoc />
    public Func<IEnumerator<T>, IEnumerator<T>>? ReplacementFunction { get; set; }

    /// <inheritdoc />
    public bool PauseUpdate { get; set; }

    /// <inheritdoc />
    public bool KillOnSuccess { get; set; } = true;

    /// <inheritdoc />
    public IEnumerable<Coroutine<T>> Coroutines
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
        updateThread = new(ThreadUpdate)
        {
            IsBackground = true,
        };
        isRunning = true;
        updateThread.Start();
        mainContext = SynchronizationContext.Current;
    }

    /// <inheritdoc />
    public void Quit()
    {
        safeCoroutines.Clear();
        isRunning = false;
        updateThread?.Interrupt();
        updateThread = null;
        watch.Stop();
    }

    /// <inheritdoc />
    public void UpdateDT(T deltaTime)
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
        if (coroutines == null)
        {
            return;
        }

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
    public CoroutineHandle AddCoroutineInstance(Coroutine<T> coroutine)
    {
        ArgumentNullException.ThrowIfNull(coroutine);

        coroutine.Delay = T.Zero;
        CoroutineHandle handle = CoroutineHandle.AsHandle(coroutine);
        safeCoroutines.TryAdd(handle, coroutine);
        return handle;
    }

    /// <inheritdoc />
    public Coroutine<T>? GetCoroutine(CoroutineHandle coroutine)
    {
        if (safeCoroutines.TryGetValue(coroutine, out var value))
        {
            return value;
        }

        return null;
    }

    private void UpdateObj(object? obj)
    {
        UpdateDT((T)obj!);
    }

    private void UpdateLogic(CoroutineHandle handle, Coroutine<T> coroutine, T deltaTime)
    {
        // IF the delay for it is not zero, we remove from current delta.
        if (coroutine.Delay > T.Zero)
        {
            coroutine.Delay -= deltaTime;
        }

        // IF the delay is zero OR smaller (means we got negative time) we work on it.
        if (coroutine.Delay <= T.Zero || coroutine.Delay == T.NegativeInfinity)
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

#pragma warning disable CA1031 // Do not catch general exception types
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
            OnException?.Invoke(this, new(handle, ex));
        }
#pragma warning restore CA1031 // Do not catch general exception types
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
        try
        {
            while (isRunning)
            {
                if (PauseUpdate)
                {
                    continue;
                }

                T updateTime = T.CreateChecked(UpdateRate * OneMillisecond);
                int updateInt = int.CreateChecked(updateTime);

                Thread.Sleep(updateInt);

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
        catch (ThreadInterruptedException)
        {
        }
    }
}
#endif