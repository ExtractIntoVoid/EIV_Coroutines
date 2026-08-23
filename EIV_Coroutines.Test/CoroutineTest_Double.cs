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
        CoroutineWorkerCustom<double>.UpdateRate = 1 / 60f;
        CoroutineDoubleManager.Start();

    }

    [TearDown]
    public void Teardown()
    {
        CoroutineDoubleManager.Stop();
    }

    [Test]
    public void TestAllFuncs()
    {
        // simple inits
        if (CoroutineDoubleManager.StaticWorker == null)
        {
            CoroutineDoubleManager.Start();
        }

        CoroutineDoubleManager.StaticWorker!.Init();
        CoroutineDoubleManager.StaticWorker!.Quit();
        CoroutineDoubleManager.StaticWorker!.Init();

        // run fast
        CoroutineDoubleManager.StaticWorker!.UpdateDT(1);

        Assert.IsEmpty(CoroutineDoubleManager.StaticWorker!.Coroutines);

        Assert.False(CoroutineDoubleManager.StaticWorker!.HasAnyCoroutinesInstance());

        Coroutine<double> coroutine = new(CountingDown(), "Test2");

        var CountingDownHash = CoroutineDoubleManager.StaticWorker!.AddCoroutineInstance(coroutine);

        var cor = CoroutineDoubleManager.StaticWorker!.GetCoroutine(CountingDownHash);
        Assert.NotNull(cor);
        Assert.AreEqual(coroutine, cor);

        CoroutineDoubleManager.StaticWorker!.UpdateDT(0);

        Assert.True(CoroutineDoubleManager.StaticWorker!.HasAnyCoroutinesInstance());

        cor = CoroutineDoubleManager.StaticWorker!.GetCoroutine(CountingDownHash);

        Console.WriteLine(cor);

        Assert.True(CoroutineDoubleManager.StaticWorker!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.True(CoroutineDoubleManager.StaticWorker!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineDoubleManager.StaticWorker!.IsCoroutineSuccessInstance(CountingDownHash));

        CoroutineDoubleManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);
        Assert.True(CoroutineDoubleManager.StaticWorker!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineDoubleManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);

        CoroutineDoubleManager.StaticWorker!.KillCoroutineInstance(coroutine);
        CoroutineDoubleManager.StaticWorker!.KillCoroutineInstance(coroutine);
        CoroutineDoubleManager.StaticWorker!.KillCoroutinesInstance([coroutine]);
        CoroutineDoubleManager.StaticWorker!.KillCoroutineTagInstance("Test2");

        CoroutineDoubleManager.StaticWorker!.UpdateDT(1);

        Assert.False(CoroutineDoubleManager.StaticWorker!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.False(CoroutineDoubleManager.StaticWorker!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineDoubleManager.StaticWorker!.IsCoroutineSuccessInstance(CountingDownHash));
        Assert.False(CoroutineDoubleManager.StaticWorker!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineDoubleManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);

        cor = CoroutineDoubleManager.StaticWorker!.GetCoroutine(CountingDownHash);
        Assert.IsNull(cor);
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(CountingDown(), "Test");
        Thread.Sleep(5);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(handle, Is.Not.Zero);
            Assert.That(handle.CoroutineHash, Is.Not.Zero);
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
            Assert.That(CoroutineDoubleManager.IsCoroutineRunning(handle), Is.True);
        }
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineDoubleManager.IsCoroutineSuccess(handle) && CoroutineDoubleManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
            {
                //Log.Information("killing after 15 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(10);
        CoroutineFloatManager.KillCoroutines([handle]);
    }


    [Test]
    public void TestWaitFor()
    {
        _TestBoolValue = false;
        var handle = CoroutineDoubleManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
        var WaitAndSetTrue_handle = CoroutineDoubleManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineDoubleManager.IsCoroutineSuccess(handle) && CoroutineDoubleManager.IsCoroutineExists(handle))
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
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.False);
            Assert.That(CoroutineDoubleManager.IsCoroutineSuccess(handle), Is.False);
        }
        CoroutineDoubleManager.KillCoroutines([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineSuccess(handle), Is.False);
        CoroutineDoubleManager.KillCoroutineTag("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineDoubleManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.False);
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineSuccess(handle), Is.False);
        CoroutineDoubleManager.KillCoroutine(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineDoubleManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.False);
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineDoubleManager.HasAnyCoroutines(), Is.False);
        var handle = CoroutineDoubleManager.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineDoubleManager.KillCoroutine(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var countDown = CoroutineDoubleManager.StartCoroutine(CountingDown(), "Test");
        var waitOther = CoroutineDoubleManager.StartCoroutine(WaitUntilOtherCor2(countDown), "Test");
        Thread.Sleep(5);
        CoroutineDoubleManager.KillCoroutine(countDown);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineDoubleManager.IsCoroutineRunning(waitOther))
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
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(waitOther), Is.False);
        }
        CoroutineDoubleManager.KillCoroutines([waitOther, countDown]);
    }


    private static IEnumerator<double> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineDoubleManager.WaitUntilZero<byte>(
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
        yield return CoroutineDoubleManager.WaitUntilZero<byte>(
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
        yield return CoroutineDoubleManager.WaitUntilTrue(() => _TestBoolValue);
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
        yield return CoroutineDoubleManager.StartAfterCoroutine(coroutineHandle);
        yield return 1;
        yield break;
    }
}
#endif