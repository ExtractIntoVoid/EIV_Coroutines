using System.Diagnostics;
using System.Numerics;

namespace EIV_Coroutines.CoroutineWorkers;

public class CoroutineWorkerCustom<T> : ICoroutineWorker<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    private readonly List<(T Delay, Coroutine<T> Cor)> _DelayAndCoroutines = [];
    public List<(T Delay, Coroutine<T> Cor)> DelayAndCoroutines
    {
        get
        {
            if (MutexLock())
            {
                var to_ret = _DelayAndCoroutines;
                MutexUnLock();
                return to_ret;
            }
            return [];
        }
    }
    public object? ReplacementObject { get; set; }
    public Func<IEnumerator<T>, IEnumerator<T>>? ReplacementFunction { get; set; }
    public bool PauseUpdate { get; set; } = false;
    public bool DontKillSuccess { get; set; } = false;
    public static T UpdateRate { get; set; } = T.One;

    #region Private fields
    private Thread? UpdateThread;
    private readonly Stopwatch Watch = new();
    private T prevTime = T.Zero;
    private T accumulator = T.Zero;
    #endregion

    #region Mutex
    private readonly Mutex _mutex = new();
    public bool MutexLock()
    {
        return _mutex.WaitOne(1);
    }

    public void MutexUnLock()
    {
        _mutex.ReleaseMutex();
    }
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
        //Console.WriteLine("Main thread!");
        Update((T)obj!);
    }

    public void Update(T deltaTime)
    {
        Kill();
        if (MutexLock())
        {
            for (int i = 0; i < _DelayAndCoroutines.Count; i++)
            {
                ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) cor_delay = new();
                GetCorAndDelayRef(i, ref cor_delay);
                //Console.WriteLine("Index: {0}, DT: {1} Delay: {2}, Cor: {3}", i, deltaTime, cor_delay.DelayAndCor.Delay, cor_delay.DelayAndCor.Cor);
                if (cor_delay.DelayAndCor.Delay > T.Zero)
                    cor_delay.DelayAndCor.Delay -= deltaTime;
                if (cor_delay.DelayAndCor.Delay <= T.Zero)
                {
                    CoroutineWork(ref cor_delay);
                }
                if (T.IsNaN(cor_delay.DelayAndCor.Delay))
                {
                    if (ReplacementFunction != null)
                    {
                        cor_delay.DelayAndCor.Cor.Enumerator = ReplacementFunction(cor_delay.DelayAndCor.Cor.Enumerator);
                        CoroutineWork(ref cor_delay);
                        ReplacementFunction = null;
                    }
                }
                SetCorAndDelayRef(ref cor_delay);
            }
            MutexUnLock();
        }
        Kill();
    }
    #endregion
    #region Kills
    public void KillCoroutineInstance(CoroutineHandle coroutine)
    {
        if (MutexLock())
        {
            ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) cor_delay = new();
            GetCorAndDelayRef(coroutine, ref cor_delay);
            if (cor_delay.index == -1)
            {
                //Console.WriteLine("No Coroutine to kill! (Handle was: {0})", coroutine);
                MutexUnLock();
                return;
            }
            cor_delay.DelayAndCor.Cor.ShouldKill = true;
            //Console.WriteLine("Coroutine {0} changed ShouldKill state", cor_delay.DelayAndCor.Cor.GetHashCode());
            SetCorAndDelayRef(ref cor_delay);
            MutexUnLock();
        }
    }

    public void KillCoroutinesInstance(IList<CoroutineHandle> coroutines)
    {
        if (MutexLock())
        {
            foreach (CoroutineHandle coroutine in coroutines)
            {
                ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) cor_delay = new();
                GetCorAndDelayRef(coroutine, ref cor_delay);
                if (cor_delay.index == -1)
                {
                    //Console.WriteLine("No Coroutine to kill! (Handle was: {0})", coroutine);
                    continue;
                }
                cor_delay.DelayAndCor.Cor.ShouldKill = true;
                //Console.WriteLine("Coroutine {0} changed ShouldKill state", cor_delay.DelayAndCor.Cor.GetHashCode());
                SetCorAndDelayRef(ref cor_delay);
            }
            MutexUnLock();
        }
    }

    public void KillCoroutineTagInstance(string tag)
    {
        var cors = DelayAndCoroutines.Where(x => x.Cor.Tag == tag).Select(x => CoroutineHandle.AsHandle(x.Cor)).ToList();
        KillCoroutinesInstance(cors);
    }
    #endregion
    #region Checks
    public bool HasAnyCoroutinesInstance()
    {
        bool success = false;
        if (MutexLock())
        {
            success = DelayAndCoroutines.Count != 0;
            MutexUnLock();
        }
        return success;
    }

    public bool IsCoroutineExistsInstance(CoroutineHandle coroutine)
    {
        return GetCoroutineIndex(coroutine) != -1;
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
        if (GetCoroutineIndex(coroutine) == -1)
            return;
        if (MutexLock())
        {
            ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) cor_delay = new();
            GetCorAndDelayRef(coroutine, ref cor_delay);
            if (cor_delay.index == -1)
            {
                //Console.WriteLine("No Coroutine to Pause! (Handle was: {0})", coroutine);
                MutexUnLock();
                return;
            }
            cor_delay.DelayAndCor.Cor.IsPaused = !cor_delay.DelayAndCor.Cor.IsPaused;
            //Console.WriteLine("Coroutine {0} changed ShouldPause state", cor_delay.DelayAndCor.Cor.GetHashCode());
            SetCorAndDelayRef(ref cor_delay);
            MutexUnLock();
        }
    }
    public void AddCoroutineInstance(Coroutine<T> coroutine)
    {
        if (MutexLock())
        {
            _DelayAndCoroutines.Add((T.Zero, coroutine));
            MutexUnLock();
        }
    }
    public int GetCoroutineIndex(CoroutineHandle coroutine)
    {
        if (MutexLock())
        {
            int index = _DelayAndCoroutines.FindIndex(x => coroutine.Equals(CoroutineHandle.AsHandle(x.Cor)));
            if (index < 0)
            {
                MutexUnLock();
                return -1;
            }
            MutexUnLock();
            return index;
        }
        return -1;
    }

    public Coroutine<T>? GetCoroutine(CoroutineHandle coroutine)
    {
        if (MutexLock())
        {
            int index = _DelayAndCoroutines.FindIndex(x => coroutine.Equals(CoroutineHandle.AsHandle(x.Cor)));
            if (index == -1)
            {
                MutexUnLock();
                return null;
            }
            var cor = _DelayAndCoroutines[index].Cor;
            MutexUnLock();
            return cor;
        }
        return null;
    }
    #endregion
    #region Private stuff
    private void CoroutineWork(ref ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) ref_values)
    {
        if (ref_values.DelayAndCor.Cor.ShouldKill)
            return;
        if (ref_values.DelayAndCor.Cor.IsPaused)
            return;
        if (ref_values.DelayAndCor.Cor.IsSuccess)
            return;
        ref_values.DelayAndCor.Cor.IsRunning = true;
        if (!MoveNext(ref ref_values))
        {
            ref_values.DelayAndCor.Cor.IsRunning = false;
            ref_values.DelayAndCor.Cor.IsSuccess = true;
            if (!DontKillSuccess)
                ref_values.DelayAndCor.Cor.ShouldKill = true;
            //Console.WriteLine("Coroutine {0} changed states", ref_values.DelayAndCor.Cor);
        }
    }

    private static bool MoveNext(ref ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) ref_values)
    {
        bool result = ref_values.DelayAndCor.Cor.Enumerator.MoveNext();
        ref_values.DelayAndCor.Delay = ref_values.DelayAndCor.Cor.Enumerator.Current;
        return result;
    }
    private void Kill()
    {
        if (MutexLock())
        {
            for (int i = 0; i < _DelayAndCoroutines.Count; i++)
            {
                if (_DelayAndCoroutines[i].Cor.ShouldKill)
                {
                    _DelayAndCoroutines.RemoveAt(i);
                }
            }
            MutexUnLock();
        }
    }

    private void GetCorAndDelayRef(int index, ref ((T Delay, Coroutine<T> Cor), int index) ref_values)
    {
        if (index == -1)
            return;
        if (MutexLock())
        {
            ref_values = (_DelayAndCoroutines[index], index);
            MutexUnLock();
        }
    }

    private void GetCorAndDelayRef(CoroutineHandle coroutine, ref ((T Delay, Coroutine<T> Cor), int index) ref_values)
    {
        if (MutexLock())
        {
            int index = _DelayAndCoroutines.FindIndex(x => coroutine.Equals(CoroutineHandle.AsHandle(x.Cor)));
            if (index < 0)
            {
                ref_values = (default, index);
                MutexUnLock();
                return;
            }
            ref_values = (_DelayAndCoroutines[index], index);
            MutexUnLock();
        }
    }

    private void SetCorAndDelayRef(ref ((T Delay, Coroutine<T> Cor) DelayAndCor, int index) ref_values)
    {
        if (ref_values.index == -1)
            return;
        if (MutexLock())
        {
            _DelayAndCoroutines[ref_values.index] = ref_values.DelayAndCor;
            MutexUnLock();
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
