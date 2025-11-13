using EIV_Coroutines.CoroutineWorkers;

namespace EIV_Coroutines.Examples;

internal class Program
{
    public static void Main(string[] _)
    {
        CoroutineWorkerCustom<double>.UpdateRate = 1 / 144f;
        CoroutineDoubleManager.Start();
        CoroutineDoubleManager.Stop();
        var handle = CoroutineDoubleManager.StartCoroutine(TestExport.CountingDown(), "Test");
        while (CoroutineDoubleManager.IsCoroutineExists(handle))
        {
            Thread.Sleep(10);
        }
    }
}
