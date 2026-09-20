namespace EIV_Coroutines;

/// <summary>
/// Represent an argument when a coroutine got an exception.
/// </summary>
/// <param name="handle">The handle of the coroutine.</param>
/// <param name="ex">The exception.</param>
public sealed class CoroutineExceptionEventArgs(CoroutineHandle handle, Exception ex) : EventArgs
{
    /// <summary>
    /// Gets the coroutine handle.
    /// </summary>
    public CoroutineHandle Handle { get; } = handle;

    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; } = ex;
}
