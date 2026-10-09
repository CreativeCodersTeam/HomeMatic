using System.Net.Sockets;
using System.Xml;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Manages the event subscriptions of one monitor run at the BidCos-RF, HmIP-RF and BidCos-Wired interfaces
/// of a CCU.
/// </summary>
/// <remarks>
/// Every interface is registered with its own interface id in the form <c>hmc-&lt;session id&gt;-&lt;kind&gt;</c>.
/// The CCU echoes this id in every callback, so <see cref="TryGetKind"/> can map a callback back to its interface.
/// </remarks>
public sealed class CcuEventSubscriptions
{
    private static readonly CcuDeviceKind[] SupportedKinds =
        [CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp, CcuDeviceKind.HomeMaticWired];

    private readonly TimeSpan _subscribeTimeout;

    private readonly TimeSpan _unsubscribeTimeout;

    private string? _callbackUrl;

    private readonly IReadOnlyList<Subscription> _subscriptions;

    private readonly Dictionary<string, CcuDeviceKind> _kindsByInterfaceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CcuEventSubscriptions"/> class.
    /// </summary>
    /// <param name="apiBuilder">The builder used to create one XML-RPC API client per interface.</param>
    /// <param name="ccuUrl">The base URL of the CCU.</param>
    /// <param name="subscribeTimeout">The maximum time a single subscribe call may take.</param>
    /// <param name="unsubscribeTimeout">The maximum time a single unsubscribe call may take.</param>
    /// <exception cref="ArgumentNullException"><paramref name="apiBuilder"/> or <paramref name="ccuUrl"/> is <see langword="null"/>.</exception>
    public CcuEventSubscriptions(
        IHomeMaticXmlRpcApiBuilder apiBuilder,
        Uri ccuUrl,
        TimeSpan subscribeTimeout,
        TimeSpan unsubscribeTimeout)
    {
        Ensure.NotNull(apiBuilder);
        Ensure.NotNull(ccuUrl);

        _subscribeTimeout = subscribeTimeout;
        _unsubscribeTimeout = unsubscribeTimeout;

        SessionId = Guid.NewGuid().ToString("N")[..8];

        _subscriptions = SupportedKinds
            .Select(kind => new Subscription(
                kind,
                $"hmc-{SessionId}-{kind}",
                apiBuilder.ForUrl(new XmlRpcApiAddress(ccuUrl, kind)).Build()))
            .ToArray();

        _kindsByInterfaceId = _subscriptions.ToDictionary(x => x.InterfaceId, x => x.Kind, StringComparer.Ordinal);
    }

    /// <summary>
    /// Subscribes the callback URL at every supported interface, in parallel.
    /// </summary>
    /// <param name="callbackUrl">The URL the CCU calls back, as passed to the CCU <c>init</c> method.</param>
    /// <returns>
    /// A task whose result lists the subscribed interfaces and the failed interfaces with their error messages.
    /// A subscribe call that does not complete within the subscribe timeout fails with the error <c>timeout</c>.
    /// A response that is not XML fails with <c>no XML-RPC service on port &lt;port&gt;</c>, and a connection
    /// error with <c>connection failed on port &lt;port&gt; (&lt;reason&gt;)</c>; any other error keeps the
    /// exception message.
    /// </returns>
    /// <remarks>
    /// This method does not throw for CCU or transport errors. An interface whose subscribe call timed out is
    /// reported as failed, but <see cref="UnsubscribeAsync"/> still unsubscribes it, because the CCU may have
    /// registered the callback URL after the timeout.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="callbackUrl"/> is <see langword="null"/>, empty or white space.</exception>
    public async Task<SubscriptionResult> SubscribeAsync(string callbackUrl)
    {
        Ensure.IsNotNullOrWhitespace(callbackUrl);

        _callbackUrl = callbackUrl;

        await Task.WhenAll(_subscriptions.Select(x => SubscribeAsync(x, callbackUrl))).ConfigureAwait(false);

        return new SubscriptionResult(
            _subscriptions
                .Where(x => x.State == SubscriptionState.Subscribed)
                .Select(x => x.Kind)
                .ToArray(),
            _subscriptions
                .Where(x => x.State is SubscriptionState.Failed or SubscriptionState.SubscribeTimedOut)
                .Select(x => (x.Kind, x.Error ?? string.Empty))
                .ToArray());
    }

    /// <summary>
    /// Unsubscribes the callback URL at every interface that is subscribed or whose subscribe call timed out, in
    /// parallel.
    /// </summary>
    /// <returns>
    /// A task whose result lists the interfaces that could not be unsubscribed with their error messages, in the
    /// order of the subscriptions. An unsubscribe call that does not complete within the unsubscribe timeout fails
    /// with the error <c>timeout</c>. Other errors are described as for <see cref="SubscribeAsync(string)"/>.
    /// </returns>
    /// <remarks>
    /// An interface is unsubscribed by calling the CCU <c>init</c> method with the callback URL and an empty
    /// interface id. An interface whose subscribe call timed out is unsubscribed as well, because the CCU may have
    /// registered it late; an interface whose subscribe call failed with an error is not. Before such an interface is
    /// unsubscribed, its subscribe call is awaited for up to the unsubscribe timeout, so the CCU cannot process the
    /// unsubscribe call before the late subscribe call. An interface is not
    /// unsubscribed again after a completed call, even if that call failed, so further calls do nothing. This method
    /// is not meant to be called concurrently. It does not throw for CCU or transport errors.
    /// </remarks>
    public async Task<IReadOnlyList<(CcuDeviceKind Kind, string Error)>> UnsubscribeAsync()
    {
        var subscribed = _subscriptions
            .Where(x => x.State is SubscriptionState.Subscribed or SubscriptionState.SubscribeTimedOut)
            .ToArray();

        if (subscribed.Length == 0 || _callbackUrl is null)
        {
            return [];
        }

        await Task.WhenAll(subscribed.Select(x => UnsubscribeAsync(x, _callbackUrl))).ConfigureAwait(false);

        return subscribed
            .Where(x => x.State == SubscriptionState.UnsubscribeFailed)
            .Select(x => (x.Kind, x.Error ?? string.Empty))
            .ToArray();
    }

    /// <summary>
    /// Maps an interface id received in a CCU callback back to its interface.
    /// </summary>
    /// <param name="interfaceId">The interface id from the callback.</param>
    /// <param name="kind">When this method returns <see langword="true"/>, contains the interface the id was issued for.</param>
    /// <returns><see langword="true"/> if <paramref name="interfaceId"/> was issued by this instance; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="interfaceId"/> is <see langword="null"/>.</exception>
    public bool TryGetKind(string interfaceId, out CcuDeviceKind kind)
    {
        Ensure.NotNull(interfaceId);

        return _kindsByInterfaceId.TryGetValue(interfaceId, out kind);
    }

    private async Task SubscribeAsync(Subscription subscription, string callbackUrl)
    {
        Task? subscribeTask = null;

        try
        {
            subscribeTask = subscription.Api.InitAsync(callbackUrl, subscription.InterfaceId);

            await subscribeTask.WaitAsync(_subscribeTimeout).ConfigureAwait(false);

            subscription.SetState(SubscriptionState.Subscribed);
        }
        catch (TimeoutException)
        {
            subscription.SetState(SubscriptionState.SubscribeTimedOut, "timeout");
            subscription.LateSubscribe = subscribeTask;
        }
        catch (Exception ex)
        {
            subscription.SetState(SubscriptionState.Failed, DescribeError(ex, subscription.Kind));
        }
    }

    private async Task UnsubscribeAsync(Subscription subscription, string callbackUrl)
    {
        if (subscription.LateSubscribe is not null)
        {
            await WaitForLateSubscribeAsync(subscription.LateSubscribe).ConfigureAwait(false);
        }

        try
        {
            await subscription.Api
                .InitAsync(callbackUrl, string.Empty)
                .WaitAsync(_unsubscribeTimeout)
                .ConfigureAwait(false);

            subscription.SetState(SubscriptionState.Unsubscribed);
        }
        catch (TimeoutException)
        {
            subscription.SetState(SubscriptionState.UnsubscribeFailed, "timeout");
        }
        catch (Exception ex)
        {
            subscription.SetState(SubscriptionState.UnsubscribeFailed, DescribeError(ex, subscription.Kind));
        }
    }

    private async Task WaitForLateSubscribeAsync(Task lateSubscribe)
    {
        try
        {
            await lateSubscribe.WaitAsync(_unsubscribeTimeout).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Whether the subscribe call completed, failed or is still running, the interface is unsubscribed anyway.
            ObserveFault(lateSubscribe);
        }
    }

    private static void ObserveFault(Task task)
    {
        task.ContinueWith(t => _ = t.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private static string DescribeError(Exception exception, CcuDeviceKind kind)
    {
        return exception switch
        {
            // The response could not be parsed as XML, e.g. an HTML page: no XML-RPC server listens on that port.
            XmlException => $"no XML-RPC service on port {kind.ToPort()}",
            HttpRequestException or SocketException =>
                $"connection failed on port {kind.ToPort()} ({exception.GetBaseException().Message})",
            _ => exception.Message
        };
    }

    /// <summary>
    /// Gets the session id that makes the interface ids of this instance unique.
    /// </summary>
    /// <value>Eight lower-case hexadecimal characters.</value>
    public string SessionId { get; }

    private enum SubscriptionState
    {
        Pending,
        Subscribed,
        Failed,
        SubscribeTimedOut,
        Unsubscribed,
        UnsubscribeFailed
    }

    private sealed class Subscription(CcuDeviceKind kind, string interfaceId, IHomeMaticXmlRpcApi api)
    {
        public void SetState(SubscriptionState state, string? error = null)
        {
            State = state;
            Error = error;
        }

        public CcuDeviceKind Kind { get; } = kind;

        public string InterfaceId { get; } = interfaceId;

        public IHomeMaticXmlRpcApi Api { get; } = api;

        public SubscriptionState State { get; private set; } = SubscriptionState.Pending;

        public string? Error { get; private set; }

        public Task? LateSubscribe { get; set; }
    }
}
