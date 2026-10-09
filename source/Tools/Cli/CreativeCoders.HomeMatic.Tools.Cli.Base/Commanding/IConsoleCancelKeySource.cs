namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

/// <summary>
/// Defines a source that notifies a long-running command when the user presses Ctrl+C.
/// </summary>
public interface IConsoleCancelKeySource
{
    /// <summary>
    /// Registers a callback that is invoked when the cancel key combination is pressed, instead of terminating
    /// the process.
    /// </summary>
    /// <param name="onCancel">The callback to invoke on every cancel key press.</param>
    /// <returns>A registration that removes the callback and restores the default behavior when disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="onCancel"/> is <see langword="null"/>.</exception>
    IDisposable Register(Action onCancel);
}
