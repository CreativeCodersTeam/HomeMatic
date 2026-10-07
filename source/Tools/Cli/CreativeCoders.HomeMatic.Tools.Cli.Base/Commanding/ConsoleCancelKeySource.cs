using CreativeCoders.Core;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;

/// <summary>
/// Notifies registered callbacks about Ctrl+C by handling <see cref="Console.CancelKeyPress"/>.
/// </summary>
/// <remarks>
/// While a callback is registered, Ctrl+C no longer terminates the process, so the command can stop cleanly.
/// </remarks>
[UsedImplicitly]
public sealed class ConsoleCancelKeySource : IConsoleCancelKeySource
{
    /// <inheritdoc />
    public IDisposable Register(Action onCancel)
    {
        Ensure.NotNull(onCancel);

        ConsoleCancelEventHandler handler = (_, e) =>
        {
            e.Cancel = true;
            onCancel();
        };

        Console.CancelKeyPress += handler;

        return new DelegateDisposable(() => Console.CancelKeyPress -= handler, true);
    }
}
