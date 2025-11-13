using EIV_Coroutines.CoroutineWorkers;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Float
{
    [OneTimeSetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineWorkerCustom<float>.UpdateRate = 1 / 144f;
        CoroutineFloatManager.Start();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        CoroutineFloatManager.Stop();
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineFloatManager.StartCoroutine(CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(100);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineRunning(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineFloatManager.IsCoroutineSuccess(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();

        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.False);
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.False);

    }


    [Test]
    public void TestWaitFor()
    {
        var handle = CoroutineFloatManager.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(true));
        var WaitAndSetTrue_handle = CoroutineFloatManager.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineFloatManager.IsCoroutineSuccess(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(100);
        Assert.That(_TestBoolValue, Is.True);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(false));
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineFloatManager.KillCoroutines([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineFloatManager.KillCoroutineTag("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineFloatManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineFloatManager.KillCoroutine(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineFloatManager.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineFloatManager.HasAnyCoroutines(), Is.EqualTo(false));
        var handle = CoroutineFloatManager.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineFloatManager.KillCoroutine(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var handle = CoroutineFloatManager.StartCoroutine(CountingDown(), "Test");
        var handle2 = CoroutineFloatManager.StartCoroutine(WaitUntilOtherCor2(handle), "Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineFloatManager.IsCoroutineSuccess(handle2))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(100);
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle2), Is.EqualTo(false));
        Assert.That(CoroutineFloatManager.IsCoroutineExists(handle), Is.EqualTo(false));
    }


    public static IEnumerator<float> CountingDown()
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
    public static IEnumerator<float> FakeCountingDown()
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

    public IEnumerator<float> WaitForTrue()
    {
        //Log.Information("_WaitForTrue! ");
        yield return CoroutineFloatManager.WaitUntilTrue(() => _TestBoolValue);
        //Log.Information("true! " + _TestBoolValue);
        yield return 0;
        yield break;
    }

    public IEnumerator<float> WaitAndSetTrue()
    {
        yield return 2;
        _TestBoolValue = true;
        yield break;
    }

    public IEnumerator<float> WaitUntilOtherCor2(CoroutineHandle coroutineHandle)
    {
        yield return 2;
        yield return CoroutineFloatManager.StartAfterCoroutine(coroutineHandle);
        yield return 1;
        yield break;
    }
}
