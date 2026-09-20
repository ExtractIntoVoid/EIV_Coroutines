#if NET8_0_OR_GREATER
using EIV_Coroutines.CoroutineWorkers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EIV_Coroutines.Examples;

internal class TestExport
{

    [UnmanagedCallersOnly(EntryPoint = "Setup", CallConvs = [typeof(CallConvCdecl)])]
    public static void Setup()
    {
        CoroutineManager.Start<double>();
        (CoroutineManager.GetWorker<double>() as CoroutineWorkerCustom<double>)?.UpdateRate = 1 / 144f;
    }

    [UnmanagedCallersOnly(EntryPoint = "Teardown", CallConvs = [typeof(CallConvCdecl)])]
    public static void Teardown()
    {
        CoroutineManager.Stop<double>();
    }


    [UnmanagedCallersOnly(EntryPoint = "Test", CallConvs = [typeof(CallConvCdecl)])]
    public static void Test()
    {
        CoroutineManager.StartCoroutine(CountingDown(), "Test");
    }

    public static IEnumerator<double> CountingDown()
    {
        yield return 0;
        byte i = byte.MaxValue;
        Console.WriteLine("_CountingDown set i to byte max");
        yield return CoroutineManager.WaitUntilZero<double, byte>(
            () =>
            {
                i--;
                Console.WriteLine("i: "+i);
                return i;
            });
        yield return 0;
        Console.WriteLine("_CountingDown bye bye");
        yield break;
    }
}
#endif