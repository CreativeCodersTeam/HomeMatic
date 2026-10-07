using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using JetBrains.Annotations;
using Spectre.Console;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Subscribes to the events of all supported interfaces of a stored CCU and prints one line per event.
/// </summary>
/// <param name="console">The console used for all output.</param>
/// <param name="ccuConnectionsStore">The store that provides the CCU connections and their credentials.</param>
/// <param name="xmlRpcApiBuilder">The builder used to create the XML-RPC clients that subscribe the interfaces.</param>
/// <param name="jsonRpcClientBuilder">The builder used to create the JSON-RPC client that loads the channel names.</param>
/// <param name="eventServerFactory">The factory that creates the server receiving the CCU callbacks.</param>
/// <param name="endpointResolver">The resolver that determines the local callback endpoint.</param>
/// <param name="cancelKeySource">The source that reports Ctrl+C, so the monitor can stop cleanly.</param>
[UsedImplicitly]
[CliCommand([CcuCommandGroup.Name, "events"],
    Description = "Subscribe to the events of a CCU and print them until Ctrl+C, Q or Esc is pressed")]
public class MonitorCcuEventsCommand(
    IAnsiConsole console,
    ICcuConnectionsStore ccuConnectionsStore,
    IHomeMaticXmlRpcApiBuilder xmlRpcApiBuilder,
    IHomeMaticJsonRpcClientBuilder jsonRpcClientBuilder,
    ICcuXmlRpcEventServerFactory eventServerFactory,
    ICallbackEndpointResolver endpointResolver,
    IConsoleCancelKeySource cancelKeySource)
    : ICliCommand<MonitorCcuEventsOptions>
{
    private const int AccessDeniedErrorCode = 5;

    private static readonly TimeSpan SubscribeTimeout = TimeSpan.FromSeconds(5);

    private static readonly TimeSpan UnsubscribeTimeout = TimeSpan.FromSeconds(2);

    private static readonly TimeSpan NameLoadTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan KeyPollInterval = TimeSpan.FromMilliseconds(50);

    // EADDRINUSE on Linux (98) and macOS (48), WSAEADDRINUSE (10048) and ERROR_ALREADY_EXISTS (183) on Windows.
    private static readonly int[] AddressInUseErrorCodes = [98, 48, 10048, 183];

    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    private readonly ICcuConnectionsStore _ccuConnectionsStore = Ensure.NotNull(ccuConnectionsStore);

    private readonly IHomeMaticXmlRpcApiBuilder _xmlRpcApiBuilder = Ensure.NotNull(xmlRpcApiBuilder);

    private readonly IHomeMaticJsonRpcClientBuilder _jsonRpcClientBuilder = Ensure.NotNull(jsonRpcClientBuilder);

    private readonly ICcuXmlRpcEventServerFactory _eventServerFactory = Ensure.NotNull(eventServerFactory);

    private readonly ICallbackEndpointResolver _endpointResolver = Ensure.NotNull(endpointResolver);

    private readonly IConsoleCancelKeySource _cancelKeySource = Ensure.NotNull(cancelKeySource);

    /// <summary>
    /// Subscribes to the events of the CCU named in <paramref name="options"/> and prints them until Ctrl+C, Q or
    /// Esc is pressed.
    /// </summary>
    /// <param name="options">The command options.</param>
    /// <returns>
    /// A task whose result is <c>0</c> after a stop, also if unsubscribing failed, or <c>-1</c> if the connection is
    /// unknown, the callback port is invalid or cannot be bound, the callback address cannot be determined, or no
    /// interface could be subscribed.
    /// </returns>
    /// <remarks>
    /// Only events that pass the address and value-key filter of <paramref name="options"/> are printed. When the
    /// output is not a terminal, for example redirected to a file or pipe, every event is written as exactly one
    /// line without wrapping at the console width.
    /// Loading the channel names is limited to ten seconds; a stop while they are loading ends the command
    /// before anything is subscribed. On a stop, the events that are still queued are printed first. Then every
    /// interface that is subscribed or whose subscribe call timed out is unsubscribed in parallel, each limited to
    /// two seconds, and the callback server is stopped. Timed-out interfaces are unsubscribed also when no interface
    /// could be subscribed.
    /// </remarks>
    public async Task<CommandResult> ExecuteAsync(MonitorCcuEventsOptions options)
    {
        using var stopSource = new CancellationTokenSource();
        using var cancelKeyRegistration = _cancelKeySource.Register(stopSource.Cancel);

        var keyWatcher = new StopKeyWatcher(_console, KeyPollInterval).RunAsync(stopSource, stopSource.Token);

        try
        {
            return await MonitorAsync(options, stopSource.Token).ConfigureAwait(false);
        }
        finally
        {
            await stopSource.CancelAsync().ConfigureAwait(false);
            await keyWatcher.ConfigureAwait(false);
        }
    }

    private async Task<CommandResult> MonitorAsync(MonitorCcuEventsOptions options, CancellationToken stopToken)
    {
        if (options.CallbackPort is < 0 or > IPEndPoint.MaxPort)
        {
            PrintError($"--callback-port must be between 0 and {IPEndPoint.MaxPort}");
            return -1;
        }

        var connections = await _ccuConnectionsStore.GetConnectionsAsync().ConfigureAwait(false);

        var connection = connections
            .FirstOrDefault(x => string.Equals(x.Name, options.Name, StringComparison.OrdinalIgnoreCase));

        if (connection is null)
        {
            PrintError($"CCU connection '{options.Name}' not found");
            return -1;
        }

        ChannelNameDirectory names;

        try
        {
            names = await LoadChannelNamesAsync(connection, stopToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stopToken.IsCancellationRequested)
        {
            // Stopped while the names were loading: nothing is subscribed yet, so there is nothing to clean up.
            _console.MarkupLine("Stopping ...");
            _console.MarkupLine("[bold lime]Event monitor stopped.[/]");

            return CommandResult.Success;
        }

        CallbackEndpoint endpoint;

        try
        {
            endpoint = _endpointResolver.Resolve(connection.Url, options.CallbackHost, options.CallbackPort);
        }
        catch (SocketException ex)
        {
            PrintError($"Cannot determine the callback address for {connection.Url.Host}: {ex.Message}. " +
                       "Use --callback-host to set it.");
            return -1;
        }
        catch (CallbackPortAllocationException ex)
        {
            PrintError($"Cannot allocate a callback port: {ex.Message}. Use --callback-port to choose a port.");
            return -1;
        }

        var filter = CcuEventFilter.Create(options.Addresses, options.ValueKeys);

        var channel = Channel.CreateUnbounded<CcuEventRecord>(new UnboundedChannelOptions { SingleReader = true });

        var subscriptions =
            new CcuEventSubscriptions(_xmlRpcApiBuilder, connection.Url, SubscribeTimeout, UnsubscribeTimeout);

        var eventServer = _eventServerFactory.Create(endpoint.ListenPrefix);

        await using (eventServer.ConfigureAwait(false))
        {
            eventServer.RegisterEventHandler(
                new CcuEventChannelHandler(subscriptions, channel.Writer, TimeProvider.System));

            if (!await TryStartEventServerAsync(eventServer, endpoint).ConfigureAwait(false))
            {
                return -1;
            }

            var subscriptionResult = await subscriptions.SubscribeAsync(endpoint.CallbackUrl).ConfigureAwait(false);

            foreach (var (kind, error) in subscriptionResult.Failed)
            {
                PrintWarning($"Interface {CcuEventLineFormatter.InterfaceLabel(kind)} not available: {error}");
            }

            if (subscriptionResult.Subscribed.Count == 0)
            {
                // Interfaces whose subscribe call timed out may have been registered late by the CCU.
                await UnsubscribeAsync(subscriptions).ConfigureAwait(false);

                PrintError($"No CCU interface could be subscribed. Is the CCU reachable at {connection.Url}?");
                return -1;
            }

            try
            {
                if (!stopToken.IsCancellationRequested)
                {
                    PrintStartup(connection, subscriptionResult.Subscribed, endpoint, filter);

                    await PrintEventsAsync(channel.Reader, names, filter, stopToken).ConfigureAwait(false);
                }
            }
            finally
            {
                // Completing the writer first makes the drain finite; later callbacks are dropped by the handler.
                channel.Writer.TryComplete();
                PrintQueuedEvents(channel.Reader, names, filter);

                _console.MarkupLine("Stopping ...");

                await UnsubscribeAsync(subscriptions).ConfigureAwait(false);
            }
        }

        _console.MarkupLine("[bold lime]Event monitor stopped.[/]");

        return CommandResult.Success;
    }

    private async Task UnsubscribeAsync(CcuEventSubscriptions subscriptions)
    {
        var failures = await subscriptions.UnsubscribeAsync().ConfigureAwait(false);

        foreach (var (kind, error) in failures)
        {
            PrintWarning($"Unsubscribe from {CcuEventLineFormatter.InterfaceLabel(kind)} failed: {error}");
        }
    }

    private async Task<ChannelNameDirectory> LoadChannelNamesAsync(
        CcuConnectionInfo connection,
        CancellationToken stopToken)
    {
        ChannelNameDirectory names;
        string? error;

        try
        {
            var credential = _ccuConnectionsStore.GetCredentials(connection);

            var client = _jsonRpcClientBuilder
                .ForUrl(connection.Url)
                .WithCredentials(credential)
                .Build();

            (names, error) = await ChannelNameDirectory
                .LoadAsync(client, NameLoadTimeout, stopToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stopToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            (names, error) = (ChannelNameDirectory.Unavailable, ex.Message);
        }

        if (error is not null)
        {
            PrintWarning($"Device names unavailable: {error}. Showing addresses only.");
        }

        return names;
    }

    private async Task<bool> TryStartEventServerAsync(ICcuXmlRpcEventServer eventServer, CallbackEndpoint endpoint)
    {
        try
        {
            await eventServer.StartAsync().ConfigureAwait(false);

            return true;
        }
        catch (HttpListenerException ex)
        {
            PrintError(ex.ErrorCode switch
            {
                _ when AddressInUseErrorCodes.Contains(ex.ErrorCode) =>
                    $"Callback port {endpoint.Port} is already in use. Use --callback-port to choose another port.",
                AccessDeniedErrorCode =>
                    $"Not allowed to listen on port {endpoint.Port}. Run as administrator or add a URL ACL: " +
                    $"netsh http add urlacl url={endpoint.ListenPrefix} " +
                    $@"user={Environment.UserDomainName}\{Environment.UserName}",
                _ => $"Cannot listen on callback port {endpoint.Port}: {ex.Message}"
            });

            return false;
        }
    }

    private void PrintStartup(
        CcuConnectionInfo connection,
        IEnumerable<CcuDeviceKind> subscribedKinds,
        CallbackEndpoint endpoint,
        CcuEventFilter filter)
    {
        var interfaces = string.Join(", ", subscribedKinds.Select(CcuEventLineFormatter.InterfaceLabel));

        _console.MarkupLine(Markup.Escape($"Listening for events of CCU '{connection.Name}' ({connection.Url})"));
        _console.MarkupLine(Markup.Escape($"  Interfaces : {interfaces}"));
        _console.MarkupLine(Markup.Escape($"  Callback   : {endpoint.CallbackUrl}"));

        if (!filter.IsEmpty)
        {
            _console.MarkupLine(Markup.Escape($"  Filter     : {FormatFilter(filter)}"));
        }

        _console.MarkupLine("  Press Ctrl+C, Q or Esc to stop.");
    }

    private static string FormatFilter(CcuEventFilter filter)
    {
        var parts = new List<string>();

        if (filter.Addresses.Count > 0)
        {
            parts.Add($"address {string.Join(", ", filter.Addresses)}");
        }

        if (filter.ValueKeys.Count > 0)
        {
            parts.Add($"value key {string.Join(", ", filter.ValueKeys)}");
        }

        return string.Join(" | ", parts);
    }

    private async Task PrintEventsAsync(
        ChannelReader<CcuEventRecord> reader,
        ChannelNameDirectory names,
        CcuEventFilter filter,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var record in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                PrintEvent(record, names, filter);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // A stop request ends the loop; this is the normal way to stop the monitor.
        }
    }

    private void PrintQueuedEvents(
        ChannelReader<CcuEventRecord> reader,
        ChannelNameDirectory names,
        CcuEventFilter filter)
    {
        while (reader.TryRead(out var record))
        {
            PrintEvent(record, names, filter);
        }
    }

    private void PrintEvent(CcuEventRecord record, ChannelNameDirectory names, CcuEventFilter filter)
    {
        if (!filter.Matches(record.Address, record.ValueKey))
        {
            return;
        }

        var line = CcuEventLineFormatter.Format(record, names);

        if (_console.Profile.Out.IsTerminal)
        {
            _console.MarkupLine(line);
            return;
        }

        // Spectre.Console wraps at the profile width, which is 80 for redirected output. Writing the plain text
        // directly keeps exactly one line per event in a file or pipe.
        _console.Profile.Out.Writer.WriteLine(Markup.Remove(line));
    }

    private void PrintError(string message)
    {
        _console.MarkupLine($"[bold italic red3]{Markup.Escape(message)}[/]");
    }

    private void PrintWarning(string message)
    {
        _console.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }
}
