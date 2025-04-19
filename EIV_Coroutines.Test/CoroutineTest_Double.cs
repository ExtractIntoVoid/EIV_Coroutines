using EIV_Coroutines.CoroutineWorkers;
using EIV_Coroutines.Extensions;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Double
{
    [OneTimeSetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineWorkerCustom<double>.UpdateRate = 1 / 144f;
        CoroutineStaticExt<double>.Start();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        CoroutineStaticExt<double>.Stop();
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineStaticExt<double>.StartCoroutine(_CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(100);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineRunning(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineStaticExt<double>.IsCoroutineSuccess(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();

        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.False);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineSuccess(handle), Is.False);

    }


    [Test]
    public void TestWaitFor()
    {
        var handle = CoroutineStaticExt<double>.StartCoroutine(_WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(true));
        var WaitAndSetTrue_handle = CoroutineStaticExt<double>.StartCoroutine(_WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineStaticExt<double>.IsCoroutineSuccess(handle))
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
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(false));
        Assert.That(CoroutineStaticExt<double>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        //CoroutineStaticExt.KillCoroutines([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineStaticExt<double>.StartCoroutine(_FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineStaticExt<double>.KillCoroutineTag("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineStaticExt<double>.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineStaticExt<double>.StartCoroutine(_FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineStaticExt<double>.KillCoroutine(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineStaticExt<double>.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<double>.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineStaticExt<double>.HasAnyCoroutines(), Is.EqualTo(false));
        var handle = CoroutineStaticExt<double>.StartCoroutine(_FakeCountingDown(), "Test");
        CoroutineStaticExt<double>.KillCoroutine(handle);
    }


    public IEnumerator<double> _CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineExt<double>.WaitUntilZero<byte>(
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
    public IEnumerator<double> _FakeCountingDown()
    {
        yield return CoroutineExt<double>.WaitUntilZero<byte>(
            () =>
            {
                return 1;
            });
        yield return 0;
        yield break;
    }

    private bool _TestBoolValue = false;

    public IEnumerator<double> _WaitForTrue()
    {
        //Log.Information("_WaitForTrue! ");
        yield return CoroutineExt<double>.WaitUntilTrue(() => _TestBoolValue);
        //Log.Information("true! " + _TestBoolValue);
        yield return 0;
        yield break;
    }

    public IEnumerator<double> _WaitAndSetTrue()
    {
        yield return 2;
        _TestBoolValue = true;
        yield break;
    }
}
