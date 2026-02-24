using EIV_Coroutines.CoroutineWorkers;

namespace EIV_Coroutines.Examples;

internal class Program
{
    public static void Main(string[] _)
    {
#if NET47
        CoroutineWorkerCustom.UpdateRate = 1 / 144f;
        CoroutineFloatManager.Start();
        CoroutineFloatManager.Stop();
        var handle = CoroutineFloatManager.StartCoroutine(CountingDown(), "Test");
        while (CoroutineFloatManager.IsCoroutineExists(handle))
        {
            Thread.Sleep(10);
        }
#else
        CoroutineWorkerCustom<double>.UpdateRate = 1 / 144f;
        CoroutineDoubleManager.Start();
        CoroutineDoubleManager.Stop();
        var handle = CoroutineDoubleManager.StartCoroutine(TestExport.CountingDown(), "Test");
        while (CoroutineDoubleManager.IsCoroutineExists(handle))
        {
            Thread.Sleep(10);
        }
#endif
    }

#if NET47
    public static IEnumerator<float> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        Console.WriteLine("_CountingDown set i to byte max");
        yield return CoroutineFloatManager.WaitUntilTrue(
            () =>
            {
                i--;
                Console.WriteLine("i: " + i);
                return i == 0;
            });
        yield return 0;
        Console.WriteLine("_CountingDown bye bye");
        yield break;
    }
#endif
}
