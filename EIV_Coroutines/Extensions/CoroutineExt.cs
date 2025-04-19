using System.Numerics;

namespace EIV_Coroutines.Extensions;

public static class CoroutineExt<T> where T : IFloatingPoint<T>, IFloatingPointIeee754<T>
{
    public static T WaitUntilFalse(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || !evaluatorFunc())
        {
            return T.Zero;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = evaluatorFunc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = CoroutineEnumeratorExt<T>.WaitUntilFalseHelper;
        return T.NaN;
    }

    public static T WaitUntilTrue(Func<bool> evaluatorFunc)
    {
        if (evaluatorFunc == null || evaluatorFunc())
        {
            return T.Zero;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = evaluatorFunc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = CoroutineEnumeratorExt<T>.WaitUntilTrueHelper;
        return T.NaN;
    }

    public static T WaitUntilZero<Number>(Func<Number> evaluatorFunc) where Number : INumber<Number>
    {
        if (evaluatorFunc() == Number.Zero)
        {
            return T.Zero;
        }
        CoroutineStaticExt<T>.StartIfNotExists();
        CoroutineStaticExt<T>.StaticWorker!.ReplacementObject = evaluatorFunc;
        CoroutineStaticExt<T>.StaticWorker!.ReplacementFunction = CoroutineEnumeratorExt<T>.WaitUntilTHelper<Number>;
        return T.NaN;
    }

    public static T StartAfterCoroutine(CoroutineHandle coroutine)
    {
        return T.Zero;
        /*
        Coroutine cor = Instance.CustomCoroutines.FirstOrDefault(x => coroutine.Equals((CoroutineHandle)x));
        if (cor.IsSuccess)
        {
            return 0;
        }
        CoroutineStaticExt.StartIfNotExists();
        CoroutineStaticExt.StaticWorker!.ReplacementObject = cor;
        CoroutineStaticExt.StaticWorker!.ReplacementFunction = new Func<IEnumerator<double>, IEnumerator<double>>(CoroutineEnumeratorExt.StartAfterCoroutineHelper);
        return double.NaN;
        */
    }
}
