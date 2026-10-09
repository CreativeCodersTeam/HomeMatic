using System.Threading.Channels;
using CreativeCoders.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Server;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;

/// <summary>
/// Receives CCU callbacks and writes the value events of the subscribed interfaces to a channel.
/// </summary>
/// <remarks>
/// Events with an interface id that is <see langword="null"/> or was not issued by the subscriptions, events with a
/// <see langword="null"/>, empty or whitespace address or value key, and events the channel does not accept, for
/// example because it is full or completed, are dropped. A <see langword="null"/> value is passed on unchanged.
/// Device management callbacks are acknowledged and otherwise ignored.
/// </remarks>
/// <param name="subscriptions">The subscriptions used to map interface ids back to interfaces.</param>
/// <param name="writer">The channel writer that receives the events.</param>
/// <param name="timeProvider">The time provider that supplies the local receive time of each event.</param>
public sealed class CcuEventChannelHandler(
    CcuEventSubscriptions subscriptions,
    ChannelWriter<CcuEventRecord> writer,
    TimeProvider timeProvider)
    : ICcuEventHandler
{
    private readonly CcuEventSubscriptions _subscriptions = Ensure.NotNull(subscriptions);

    private readonly ChannelWriter<CcuEventRecord> _writer = Ensure.NotNull(writer);

    private readonly TimeProvider _timeProvider = Ensure.NotNull(timeProvider);

    /// <inheritdoc />
    public Task Event(string interfaceId, string address, string valueKey, object value)
    {
        if (interfaceId is not null
            && !string.IsNullOrWhiteSpace(address)
            && !string.IsNullOrWhiteSpace(valueKey)
            && _subscriptions.TryGetKind(interfaceId, out var kind))
        {
            _writer.TryWrite(new CcuEventRecord(_timeProvider.GetLocalNow(), kind, address, valueKey, value));
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NewDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteDevices(string interfaceId, DeviceDescription[] deviceDescriptions)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateDevice(string interfaceId, string address, int hint)
    {
        return Task.CompletedTask;
    }
}
