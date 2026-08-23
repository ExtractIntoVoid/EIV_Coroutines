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
        CoroutineWorkerCustom<float>.UpdateRate = 1 / 60f;
        CoroutineFloatManager.Start();
        TestContext.Progress.WriteLine("Setup");

    }

    [TearDown]
    public void Teardown()
    {
        TestContext.Progress.WriteLine("Teardown");
        CoroutineFloatManager.Stop();
    }

    [Test]
    public void TestAllFuncs()
    {
        // simple inits
        if (CoroutineFloatManager.StaticWorker == null)
        {
            CoroutineFloatManager.Start();
        }

        CoroutineFloatManager.StaticWorker!.Init();
        CoroutineFloatManager.StaticWorker!.Quit();
        CoroutineFloatManager.StaticWorker!.Init();
        
        // run fast
        CoroutineFloatManager.StaticWorker!.UpdateDT(1);

        Assert.IsEmpty(CoroutineFloatManager.StaticWorker!.Coroutines);

        Assert.False(CoroutineFloatManager.StaticWorker!.HasAnyCoroutinesInstance());

        Coroutine<float> coroutine = new(CountingDown(), "Test");

        var CountingDownHash = CoroutineFloatManager.StaticWorker!.AddCoroutineInstance(coroutine);

        var cor = CoroutineFloatManager.StaticWorker!.GetCoroutine(CountingDownHash);
        Assert.NotNull(cor);
        Assert.AreEqual(coroutine, cor);

        CoroutineFloatManager.StaticWorker!.UpdateDT(1);

        Assert.True(CoroutineFloatManager.StaticWorker!.HasAnyCoroutinesInstance());

        Assert.True(CoroutineFloatManager.StaticWorker!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.True(CoroutineFloatManager.StaticWorker!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineFloatManager.StaticWorker!.IsCoroutineSuccessInstance(CountingDownHash));

        CoroutineFloatManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);
        Assert.True(CoroutineFloatManager.StaticWorker!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineFloatManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);

        CoroutineFloatManager.StaticWorker!.KillCoroutineInstance(coroutine);
        CoroutineFloatManager.StaticWorker!.KillCoroutineInstance(coroutine);
        CoroutineFloatManager.StaticWorker!.KillCoroutinesInstance([coroutine]);
        CoroutineFloatManager.StaticWorker!.KillCoroutineTagInstance("Test");

        CoroutineFloatManager.StaticWorker!.UpdateDT(1);

        Assert.False(CoroutineFloatManager.StaticWorker!.IsCoroutineExistsInstance(CountingDownHash));
        Assert.False(CoroutineFloatManager.StaticWorker!.IsCoroutineRunningInstance(CountingDownHash));
        Assert.False(CoroutineFloatManager.StaticWorker!.IsCoroutineSuccessInstance(CountingDownHash));
        Assert.False(CoroutineFloatManager.StaticWorker!.IsCoroutinePausedInstance(CountingDownHash));
        CoroutineFloatManager.StaticWorker!.PauseCoroutineInstance(CountingDownHash);

        cor = CoroutineFloatManager.StaticWorker!.GetCoroutine(CountingDownHash);
        Assert.IsNull(cor);
    }


    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineFloatManager.StartCoroutine(CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(100);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineRunning(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineFloatManager.IsCoroutineSuccess(handle))
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
        var handle = CoroutineFloatManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.True);
        var WaitAndSetTrue_handle = CoroutineFloatManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineFloatManager.IsCoroutineSuccess(handle) && CoroutineFloatManager.IsCoroutineExists(handle))
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
            Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.False);
            Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.False);
        }
        CoroutineFloatManager.KillCoroutines([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.False);
        CoroutineFloatManager.KillCoroutineTag("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineFloatManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.False);
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.False);
        CoroutineFloatManager.KillCoroutine(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineFloatManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.False);
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineFloatManager.HasAnyCoroutines(), Is.False);
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineFloatManager.KillCoroutine(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var countDown = CoroutineFloatManager.StartCoroutine(CountingDown(), "Test");
        var waitOther = CoroutineFloatManager.StartCoroutine(WaitUntilOtherCor2(countDown), "Test");
        CoroutineDoubleManager.KillCoroutine(countDown);
        Thread.Sleep(10);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineFloatManager.IsCoroutineExists(waitOther))
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
            Assert.That(CoroutineFloatManager.IsCoroutineExists(waitOther), Is.False);
        }
        CoroutineFloatManager.KillCoroutines([waitOther, countDown]);
    }


    private static IEnumerator<float> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineFloatManager.WaitUntilZero<byte>(
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
        yield return CoroutineFloatManager.WaitUntilZero<byte>(
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
        yield return CoroutineFloatManager.WaitUntilTrue(() => _TestBoolValue);
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
        yield return CoroutineFloatManager.StartAfterCoroutine(coroutineHandle);
        yield return 1;
        yield break;
    }
}
#endif