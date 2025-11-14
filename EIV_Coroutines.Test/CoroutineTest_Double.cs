using EIV_Coroutines.CoroutineWorkers;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Double
{
    [OneTimeSetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineWorkerCustom<double>.UpdateRate = 1 / 144f;
        CoroutineDoubleManager.Start();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        CoroutineDoubleManager.Stop();
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(100);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineRunning(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineDoubleManager.IsCoroutineSuccess(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(10);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.False);
            Assert.That(CoroutineDoubleManager.IsCoroutineSuccess(handle), Is.False);
        }

    }


    [Test]
    public void TestWaitFor()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.Zero);
        Thread.Sleep(10);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.True);
        var WaitAndSetTrue_handle = CoroutineDoubleManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineDoubleManager.IsCoroutineSuccess(handle))
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
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
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
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
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
    public void TestSameCor()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(FakeCountingDown());
        var handle2 = CoroutineDoubleManager.StartCoroutine(FakeCountingDown());
        if (handle == handle2)
            Assert.Fail();
        CoroutineDoubleManager.KillCoroutine(handle);
        Thread.Sleep(1);
        Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle2), Is.True);
        CoroutineDoubleManager.KillCoroutine(handle2);
    }


    [Test]
    public void TestOtherCor()
    {
        var handle = CoroutineDoubleManager.StartCoroutine(CountingDown(), "Test");
        var handle2 = CoroutineDoubleManager.StartCoroutine(WaitUntilOtherCor2(handle), "Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineDoubleManager.IsCoroutineSuccess(handle2))
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
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle2), Is.False);
            Assert.That(CoroutineDoubleManager.IsCoroutineExists(handle), Is.False);
        }
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
        yield return 2;
        yield return CoroutineDoubleManager.StartAfterCoroutine(coroutineHandle);
        yield return 1;
        yield break;
    }
}
