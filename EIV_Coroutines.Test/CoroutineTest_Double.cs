#if NET5_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Double
{
    [SetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineManager.Start<double>();
        (CoroutineManager.GetWorker<double>() as CoroutineWorkerCustom<double>)?.UpdateRate = 1 / 60f;

    }

    [TearDown]
    public void Teardown()
    {
        CoroutineManager.Stop<double>();
    }

    [Test]
    public void TestAllFuncs()
    {
        // simple inits
        if (CoroutineManager.GetWorker<double>() == null)
        {
            CoroutineManager.Stop<double>();
        }

        CoroutineManager.GetWorker<double>()!.Init();
        CoroutineManager.GetWorker<double>()!.Quit();
        CoroutineManager.GetWorker<double>()!.Init();

        // run fast
        CoroutineManager.GetWorker<double>()!.UpdateDT(1);

        Assert.IsEmpty(CoroutineManager.GetWorker<double>()!.Coroutines);

        Assert.False(CoroutineManager.GetWorker<double>()!.HasAnyCoroutinesInstance());

        Coroutine<double> coroutine = new(CountingDown(), "Test2");

        var CountingDownHash = CoroutineManager.GetWorker<double>()!.AddCoroutineInstance(coroutine);

        var cor = CoroutineManager.GetWorker<double>()!.GetCoroutine(CountingDownHash);
        Assert.NotNull(cor);
        Assert.AreEqual(coroutine, cor);

        CoroutineManager.GetWorker<double>()!.UpdateDT(0);

        Assert.True(CoroutineManager.GetWorker<double>()!.HasAnyCoroutinesInstance());

        cor = CoroutineManager.GetWorker<double>()!.GetCoroutine(CountingDownHash);

        Console.WriteLine(cor);

        Assert.True(CoroutineManager.GetWorker<double>()!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.True(CoroutineManager.GetWorker<double>()!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<double>()!.IsCoroutineSuccessInstance(CountingDownHash));

        CoroutineManager.GetWorker<double>()!.PauseCoroutineInstance(CountingDownHash);
        Assert.True(CoroutineManager.GetWorker<double>()!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineManager.GetWorker<double>()!.PauseCoroutineInstance(CountingDownHash);

        CoroutineManager.GetWorker<double>()!.KillCoroutineInstance(coroutine);
        CoroutineManager.GetWorker<double>()!.KillCoroutineInstance(coroutine);
        CoroutineManager.GetWorker<double>()!.KillCoroutinesInstance([coroutine]);
        CoroutineManager.GetWorker<double>()!.KillCoroutineTagInstance("Test2");

        CoroutineManager.GetWorker<double>()!.UpdateDT(1);

        Assert.False(CoroutineManager.GetWorker<double>()!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<double>()!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<double>()!.IsCoroutineSuccessInstance(CountingDownHash));
        Assert.False(CoroutineManager.GetWorker<double>()!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineManager.GetWorker<double>()!.PauseCoroutineInstance(CountingDownHash);

        cor = CoroutineManager.GetWorker<double>()!.GetCoroutine(CountingDownHash);
        Assert.IsNull(cor);
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineManager.StartCoroutine(CountingDown(), "Test");
        Thread.Sleep(5);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(handle, Is.Not.Zero);
            Assert.That(handle.CoroutineHash, Is.Not.Zero);
            Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.True);
            Assert.That(CoroutineManager.IsCoroutineRunning<double>(handle), Is.True);
        }
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineManager.IsCoroutineSuccess<double>(handle) && CoroutineManager.IsCoroutineExists<double>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
            {
                //Log.Information("killing after 15 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(10);
        CoroutineManager.KillCoroutines<double>([handle]);
    }


    [Test]
    public void TestWaitFor()
    {
        _TestBoolValue = false;
        var handle = CoroutineManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.True);
        var WaitAndSetTrue_handle = CoroutineManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineManager.IsCoroutineSuccess<double>(handle) && CoroutineManager.IsCoroutineExists<double>(handle))
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
            Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.False);
            Assert.That(CoroutineManager.IsCoroutineSuccess<double>(handle), Is.False);
        }
        CoroutineManager.KillCoroutines<double>([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineSuccess<double>(handle), Is.False);
        CoroutineManager.KillCoroutineTag<double>("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineExists<double>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.False);
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineSuccess<double>(handle), Is.False);
        CoroutineManager.KillCoroutine<double>(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineExists<double>(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineManager.IsCoroutineExists<double>(handle), Is.False);
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineManager.HasAnyCoroutines<double>(), Is.False);
        var handle = CoroutineManager.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineManager.KillCoroutine<double>(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var countDown = CoroutineManager.StartCoroutine(CountingDown(), "Test");
        var waitOther = CoroutineManager.StartCoroutine(WaitUntilOtherCor2(countDown), "Test");
        Thread.Sleep(5);
        CoroutineManager.KillCoroutine<double>(countDown);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineManager.IsCoroutineRunning<double>(waitOther))
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
            Assert.That(CoroutineManager.IsCoroutineExists<double>(waitOther), Is.False);
        }
        CoroutineManager.KillCoroutines<double>([waitOther, countDown]);
    }


    private static IEnumerator<double> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineManager.WaitUntilZero<double, byte>(
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
    private static IEnumerator<double> FakeCountingDown()
    {
        yield return CoroutineManager.WaitUntilZero<double, byte>(
            () =>
            {
                return 1;
            });
        yield return 0;
        yield break;
    }

    private bool _TestBoolValue = false;

    private IEnumerator<double> WaitForTrue()
    {
        //Log.Information("_WaitForTrue! ");
        yield return CoroutineManager.WaitUntilTrue<double>(() => _TestBoolValue);
        //Log.Information("true! " + _TestBoolValue);
        yield return 0;
        yield break;
    }

    private IEnumerator<double> WaitAndSetTrue()
    {
        yield return 2;
        _TestBoolValue = true;
        yield break;
    }

    private static IEnumerator<double> WaitUntilOtherCor2(CoroutineHandle coroutineHandle)
    {
        yield return 0;
        yield return CoroutineManager.StartAfterCoroutine<double>(coroutineHandle);
        yield return 1;
        yield break;
    }
}
#endif