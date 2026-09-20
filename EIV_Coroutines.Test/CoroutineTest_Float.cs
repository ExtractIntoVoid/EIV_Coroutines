#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Float
{
    [SetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineManager.Start<float>();
        (CoroutineManager.GetWorker<float>() as CoroutineWorkerCustom<float>)?.UpdateRate = 1 / 60f;
        TestContext.Progress.WriteLine("Setup");

    }

    [TearDown]
    public void Teardown()
    {
        TestContext.Progress.WriteLine("Teardown");
        CoroutineManager.Stop<float>();
    }

    [Test]
    public void TestAllFuncs()
    {
        // simple inits
        if (CoroutineManager.GetWorker<float>() == null)
        {
            CoroutineManager.Start<float>();
        }

        CoroutineManager.GetWorker<float>()!.Init();
        CoroutineManager.GetWorker<float>()!.Quit();
        CoroutineManager.GetWorker<float>()!.Init();
        
        // run fast
        CoroutineManager.GetWorker<float>()!.UpdateDT(1);

        Assert.IsEmpty(CoroutineManager.GetWorker<float>()!.Coroutines);

        Assert.False(CoroutineManager.GetWorker<float>()!.HasAnyCoroutinesInstance());

        Coroutine<float> coroutine = new(CountingDown(), "Test");

        var CountingDownHash = CoroutineManager.GetWorker<float>()!.AddCoroutineInstance(coroutine);

        var cor = CoroutineManager.GetWorker<float>()!.GetCoroutine(CountingDownHash);
        Assert.NotNull(cor);
        Assert.AreEqual(coroutine, cor);

        CoroutineManager.GetWorker<float>()!.UpdateDT(1);

        Assert.True(CoroutineManager.GetWorker<float>()!.HasAnyCoroutinesInstance());

        Assert.True(CoroutineManager.GetWorker<float>()!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.True(CoroutineManager.GetWorker<float>()!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<float>()!.IsCoroutineSuccessInstance(CountingDownHash));

        CoroutineManager.GetWorker<float>()!.PauseCoroutineInstance(CountingDownHash);
        Assert.True(CoroutineManager.GetWorker<float>()!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineManager.GetWorker<float>()!.PauseCoroutineInstance(CountingDownHash);

        CoroutineManager.GetWorker<float>()!.KillCoroutineInstance(coroutine);
        CoroutineManager.GetWorker<float>()!.KillCoroutineInstance(coroutine);
        CoroutineManager.GetWorker<float>()!.KillCoroutinesInstance([coroutine]);
        CoroutineManager.GetWorker<float>()!.KillCoroutineTagInstance("Test");

        CoroutineManager.GetWorker<float>()!.UpdateDT(1);

        Assert.False(CoroutineManager.GetWorker<float>()!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<float>()!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<float>()!.IsCoroutineSuccessInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<float>()!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineManager.GetWorker<float>()!.PauseCoroutineInstance(CountingDownHash);

        cor = CoroutineManager.GetWorker<float>()!.GetCoroutine(CountingDownHash);
        Assert.IsNull(cor);
    }


    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineManager.StartCoroutine(CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(100);
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineRunning<float>(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineManager.IsCoroutineSuccess<float>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
            {
                //Log.Information("killing after 15 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(10);
        CoroutineManager.KillCoroutines<float>([handle]);
    }


    [Test]
    public void TestWaitFor()
    {
        _TestBoolValue = false;
        var handle = CoroutineManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.True);
        var WaitAndSetTrue_handle = CoroutineManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineManager.IsCoroutineSuccess<float>(handle) && CoroutineManager.IsCoroutineExists<float>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(100);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_TestBoolValue, Is.True);
            Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.False);
            Assert.That(CoroutineManager.IsCoroutineSuccess<float>(handle), Is.False);
        }
        CoroutineManager.KillCoroutines<float>([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineSuccess<float>(handle), Is.False);
        CoroutineManager.KillCoroutineTag<float>("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineExists<float>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.False);
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineSuccess<float>(handle), Is.False);
        CoroutineManager.KillCoroutine<float>(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineExists<float>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<float>(handle), Is.False);
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineManager.HasAnyCoroutines<float>(), Is.False);
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineManager.KillCoroutine<float>(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var countDown = CoroutineManager.StartCoroutine(CountingDown(), "Test");
        var waitOther = CoroutineManager.StartCoroutine(WaitUntilOtherCor2(countDown), "Test");
        CoroutineManager.KillCoroutine<float>(countDown);
        Thread.Sleep(10);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineExists<float>(waitOther))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(100);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(CoroutineManager.IsCoroutineExists<float>(waitOther), Is.False);
        }
        CoroutineManager.KillCoroutines<float>([waitOther, countDown]);
    }


    private static IEnumerator<float> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineManager.WaitUntilZero<float, byte>(
            () =>
            {
                i--;
                //Log.Information("i: "+i);
                return i;
            });
        yield return 0;
        //Log.Information("_CountingDown bye bye");
        yield break;
    }
    private static IEnumerator<float> FakeCountingDown()
    {
        yield return CoroutineManager.WaitUntilZero<float, byte>(
            () =>
            {
                return 1;
            });
        yield return 0;
        yield break;
    }

    private bool _TestBoolValue = false;

    private IEnumerator<float> WaitForTrue()
    {
        //Log.Information("_WaitForTrue! ");
        yield return CoroutineManager.WaitUntilTrue<float>(() => _TestBoolValue);
        //Log.Information("true! " + _TestBoolValue);
        yield return 0;
        yield break;
    }

    private IEnumerator<float> WaitAndSetTrue()
    {
        yield return 2;
        _TestBoolValue = true;
        yield break;
    }

    private static IEnumerator<float> WaitUntilOtherCor2(CoroutineHandle coroutineHandle)
    {
        yield return 2;
        yield return CoroutineManager.StartAfterCoroutine<float>(coroutineHandle);
        yield return 1;
        yield break;
    }
}
#endif