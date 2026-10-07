using System.Threading.Channels;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class CcuEventChannelHandlerTests
{
    private const string Address = "000A1B2C3D4E5F:1";

    private static readonly DateTimeOffset Now = new(2026, 10, 6, 21, 15, 3, 412, TimeSpan.Zero);

    private readonly CcuEventSubscriptions _subscriptions = new(
        A.Fake<IHomeMaticXmlRpcApiBuilder>(), new Uri("http://ccu.example.local/"), TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(1));

    private readonly Channel<CcuEventRecord> _channel = Channel.CreateUnbounded<CcuEventRecord>();

    [Fact]
    public async Task Event_KnownInterfaceId_WritesRecordWithReceiveTime()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.Event(interfaceId, Address, "STATE", true);

        // Assert
        _channel.Reader.TryRead(out var record).Should().BeTrue();
        record.Should().Be(new CcuEventRecord(Now, CcuDeviceKind.HomeMaticIp, Address, "STATE", true));
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Event_TwoEvents_WritesThemInCallOrder()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMatic}";

        // Act
        await sut.Event(interfaceId, Address, "STATE", true);
        await sut.Event(interfaceId, Address, "LEVEL", 0.5);

        // Assert
        _channel.Reader.TryRead(out var first).Should().BeTrue();
        _channel.Reader.TryRead(out var second).Should().BeTrue();
        first!.ValueKey.Should().Be("STATE");
        second!.ValueKey.Should().Be("LEVEL");
        second.Interface.Should().Be(CcuDeviceKind.HomeMatic);
    }

    [Theory]
    [InlineData("hmc-00000000-HomeMaticIp")]
    [InlineData("")]
    public async Task Event_UnknownInterfaceId_WritesNothing(string interfaceId)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Event(interfaceId, Address, "STATE", true);

        // Assert
        await act.Should().NotThrowAsync();
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Event_NullInterfaceId_DoesNotThrowAndWritesNothing()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.Event(null!, Address, "STATE", true);

        // Assert
        await act.Should().NotThrowAsync();
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task Event_MissingAddress_DoesNotThrowAndWritesNothing(string? address)
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        var act = () => sut.Event(interfaceId, address!, "STATE", true);

        // Assert
        await act.Should().NotThrowAsync();
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task Event_MissingValueKey_DoesNotThrowAndWritesNothing(string? valueKey)
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        var act = () => sut.Event(interfaceId, Address, valueKey!, true);

        // Assert
        await act.Should().NotThrowAsync();
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Event_MissingAddressFollowedByValidEvent_WritesOnlyValidEvent()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.Event(interfaceId, null!, "STATE", true);
        await sut.Event(interfaceId, Address, "LEVEL", 0.5);

        // Assert
        _channel.Reader.TryRead(out var record).Should().BeTrue();
        record!.ValueKey.Should().Be("LEVEL");
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Event_AddressAndValueKeyWithSurroundingWhitespace_WritesThemUnchanged()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.Event(interfaceId, " " + Address, "STATE ", true);

        // Assert
        _channel.Reader.TryRead(out var record).Should().BeTrue();
        record!.Address.Should().Be(" " + Address);
        record.ValueKey.Should().Be("STATE ");
    }

    [Fact]
    public async Task Event_BoundedChannelFull_DropsEventWithoutThrowing()
    {
        // Arrange
        var channel = Channel.CreateBounded<CcuEventRecord>(
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.Wait });
        var sut = new CcuEventChannelHandler(_subscriptions, channel.Writer, new FixedTimeProvider(Now));
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";
        await sut.Event(interfaceId, Address, "STATE", true);

        // Act
        var act = () => sut.Event(interfaceId, Address, "LEVEL", 0.5);

        // Assert
        await act.Should().NotThrowAsync();
        channel.Reader.TryRead(out var record).Should().BeTrue();
        record!.ValueKey.Should().Be("STATE");
        channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Event_NullValue_WritesRecordWithNullValue()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.Event(interfaceId, Address, "STATE", null!);

        // Assert
        _channel.Reader.TryRead(out var record).Should().BeTrue();
        record.Should().Be(new CcuEventRecord(Now, CcuDeviceKind.HomeMaticIp, Address, "STATE", null));
    }

    [Fact]
    public async Task Event_LocalTimeZoneWithOffset_UsesLocalReceiveTime()
    {
        // Arrange
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("Test+02", TimeSpan.FromHours(2), "Test+02", "Test+02");
        var sut = new CcuEventChannelHandler(_subscriptions, _channel.Writer, new FixedTimeProvider(Now, timeZone));
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.Event(interfaceId, Address, "STATE", true);

        // Assert
        _channel.Reader.TryRead(out var record).Should().BeTrue();
        record!.ReceivedAt.Offset.Should().Be(TimeSpan.FromHours(2));
        record.ReceivedAt.Hour.Should().Be(23);
    }

    [Fact]
    public async Task DeviceCallbacks_Called_CompleteWithoutWriting()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        await sut.NewDevices(interfaceId, [new DeviceDescription()]);
        await sut.DeleteDevices(interfaceId, [new DeviceDescription()]);
        await sut.UpdateDevice(interfaceId, Address, 0);

        // Assert
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task AllCallbacks_ChannelWriterCompleted_DoNotThrow()
    {
        // Arrange
        var sut = CreateSut();
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";
        _channel.Writer.Complete();

        // Act
        var act = async () =>
        {
            await sut.Event(interfaceId, Address, "STATE", true);
            await sut.NewDevices(interfaceId, []);
            await sut.DeleteDevices(interfaceId, []);
            await sut.UpdateDevice(interfaceId, Address, 1);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Event_TimeProviderThrows_DoesNotThrow()
    {
        // Arrange
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Throws(new InvalidOperationException("clock broken"));
        var sut = new CcuEventChannelHandler(_subscriptions, _channel.Writer, timeProvider);
        var interfaceId = $"hmc-{_subscriptions.SessionId}-{CcuDeviceKind.HomeMaticIp}";

        // Act
        var act = () => sut.Event(interfaceId, Address, "STATE", true);

        // Assert
        await act.Should().NotThrowAsync();
        _channel.Reader.TryRead(out _).Should().BeFalse();
    }

    private CcuEventChannelHandler CreateSut()
    {
        return new CcuEventChannelHandler(_subscriptions, _channel.Writer, new FixedTimeProvider(Now));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now, TimeZoneInfo? localTimeZone = null) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => localTimeZone ?? TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow()
        {
            return now;
        }
    }
}
