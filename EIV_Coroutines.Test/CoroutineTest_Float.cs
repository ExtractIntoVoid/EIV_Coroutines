﻿using EIV_Coroutines.CoroutineWorkers;
using EIV_Coroutines.Extensions;
using System.Diagnostics;

namespace EIV_Coroutines.Test;

public class CoroutineTest_Float
{
    [OneTimeSetUp]
    public void SetUp()
    {
        // This exist here to make our test faster, running at 144 fps
        CoroutineWorkerCustom<float>.UpdateRate = 1 / 144f;
        CoroutineStaticExt<float>.Start();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        CoroutineStaticExt<float>.Stop();
    }

    [Test]
    public void TestWaitCountdown()
    {
        var handle = CoroutineStaticExt<float>.StartCoroutine(CountingDown(), "Test");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(100);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.True);
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineRunning(handle), Is.True);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineStaticExt<float>.IsCoroutineSuccess(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();

        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.False);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineSuccess(handle), Is.False);

    }


    [Test]
    public void TestWaitFor()
    {
        var handle = CoroutineStaticExt<float>.StartCoroutine(WaitForTrue(), "_WaitForTrue");
        Assert.That(handle, Is.Not.Zero);
        Assert.That(handle.CoroutineHash, Is.Not.EqualTo(0));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(true));
        var WaitAndSetTrue_handle = CoroutineStaticExt<float>.StartCoroutine(WaitAndSetTrue(), "_WaitAndSetTrue");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineStaticExt<float>.IsCoroutineSuccess(handle))
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
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(false));
        Assert.That(CoroutineStaticExt<float>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineStaticExt<float>.KillCoroutines([handle, WaitAndSetTrue_handle]);
        _TestBoolValue = false;
    }

    [Test]
    public void TestKillTag()
    {
        var handle = CoroutineStaticExt<float>.StartCoroutine(FakeCountingDown(), "Test");
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineStaticExt<float>.KillCoroutineTag("Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineStaticExt<float>.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestKill()
    {
        var handle = CoroutineStaticExt<float>.StartCoroutine(FakeCountingDown(), "Test_KILL");
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(true));
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineSuccess(handle), Is.EqualTo(false));
        CoroutineStaticExt<float>.KillCoroutine(handle);
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (CoroutineStaticExt<float>.IsCoroutineExists(handle))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                Assert.Fail();
            // wait until test over.
        }
        Thread.Sleep(10);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(false));
    }

    [Test]
    public void TestNoCor()
    {
        Assert.That(CoroutineStaticExt<float>.HasAnyCoroutines(), Is.EqualTo(false));
        var handle = CoroutineStaticExt<float>.StartCoroutine(FakeCountingDown(), "Test");
        CoroutineStaticExt<float>.KillCoroutine(handle);
    }


    [Test]
    public void TestOtherCor()
    {
        var handle = CoroutineStaticExt<float>.StartCoroutine(CountingDown(), "Test");
        var handle2 = CoroutineStaticExt<float>.StartCoroutine(WaitUntilOtherCor2(handle), "Test");
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!CoroutineStaticExt<float>.IsCoroutineSuccess(handle2))
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
            {
                //Log.Information("killing after 10 sec");
                Assert.Fail();
            }
        }
        stopwatch.Stop();
        Thread.Sleep(100);
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle2), Is.EqualTo(false));
        Assert.That(CoroutineStaticExt<float>.IsCoroutineExists(handle), Is.EqualTo(false));
    }


    public static IEnumerator<float> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        //Log.Information("_CountingDown set i to byte max");
        yield return CoroutineExt<float>.WaitUntilZero<byte>(
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
        yield return CoroutineExt<float>.WaitUntilZero<byte>(
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
        yield return CoroutineExt<float>.WaitUntilTrue(() => _TestBoolValue);
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
        yield return CoroutineExt<float>.StartAfterCoroutine(coroutineHandle);
        yield return 1;
        yield break;
    }
}
