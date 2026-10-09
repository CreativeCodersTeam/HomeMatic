using CreativeCoders.Core;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Watches the console for the stop keys Q and Esc and requests a stop when one of them is pressed.
/// </summary>
/// <remarks>
/// The watcher polls <see cref="IAnsiConsoleInput.IsKeyAvailable"/> instead of awaiting a key, because a pending
/// blocking read cannot be cancelled cleanly on every platform. It does nothing on a console that is not
/// interactive, for example when the standard input is redirected.
/// </remarks>
/// <param name="console">The console whose input is watched.</param>
/// <param name="pollInterval">The time to wait between two checks for a pressed key.</param>
public sealed class StopKeyWatcher(IAnsiConsole console, TimeSpan pollInterval)
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly TimeSpan _pollInterval = pollInterval;

    /// <summary>
    /// Watches the console until a stop key is pressed or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="stopSource">The source that is cancelled when Q or Esc is pressed.</param>
    /// <param name="cancellationToken">The token that ends the watching.</param>
    /// <returns>
    /// A task that completes when a stop key was pressed, <paramref name="cancellationToken"/> was cancelled, the
    /// console input failed, or right away if the console is not interactive.
    /// </returns>
    /// <remarks>
    /// Cancelling <paramref name="cancellationToken"/> ends the task without an exception. If the console input
    /// fails with an <see cref="InvalidOperationException"/> or <see cref="IOException"/>, watching ends without
    /// an exception and without requesting a stop.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="stopSource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The console is interactive and the poll interval is negative and not <see cref="Timeout.InfiniteTimeSpan"/>.
    /// </exception>
    public async Task RunAsync(CancellationTokenSource stopSource, CancellationToken cancellationToken)
    {
        Ensure.NotNull(stopSource);

        if (!_console.Profile.Capabilities.Interactive)
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_console.Input.IsKeyAvailable() && IsStopKey(_console.Input.ReadKey(true)))
                {
                    await stopSource.CancelAsync().ConfigureAwait(false);

                    return;
                }

                await Task.Delay(_pollInterval, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancelling the token is the normal way to end the watching.
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            // The console cannot read keys; watching ends and Ctrl+C remains the way to stop.
        }
    }

    private static bool IsStopKey(ConsoleKeyInfo? key)
    {
        return key?.Key is ConsoleKey.Q or ConsoleKey.Escape;
    }
}
