using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class MultiCcuClientTests
{
    private const string DeviceA = "ABC0001234";
    private const string DeviceB = "ABC0005678";

    [Fact]
    public async Task GetDeviceAsync_UnknownDevice_ProbesAllClientsAndThrows()
    {
        var clientA = CreateClientWithDevices();
        var clientB = CreateClientWithDevices();

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        var act = () => multi.GetDeviceAsync("UNKNOWN");

        await act.Should().ThrowAsync<KeyNotFoundException>();
        A.CallTo(() => clientA.GetDeviceAsync("UNKNOWN")).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetDeviceAsync("UNKNOWN")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetDeviceAsync_SecondCall_UsesOnlyOwningClient()
    {
        var clientA = CreateClientWithDevices(DeviceA);
        var clientB = CreateClientWithDevices(DeviceB);

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        await multi.GetDeviceAsync(DeviceB);
        await multi.GetDeviceAsync(DeviceB);

        // First call probes both until success, second call hits only clientB thanks to routing table.
        A.CallTo(() => clientA.GetDeviceAsync(DeviceB)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetDeviceAsync(DeviceB)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task GetDevicesAsync_PopulatesRoutingTableSoSubsequentGetDeviceSkipsOtherClients()
    {
        var clientA = CreateClientWithDevices(DeviceA);
        var clientB = CreateClientWithDevices(DeviceB);

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        _ = await multi.GetDevicesAsync();

        await multi.GetDeviceAsync(DeviceA);
        await multi.GetDeviceAsync(DeviceB);

        A.CallTo(() => clientA.GetDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientA.GetDeviceAsync(DeviceB)).MustNotHaveHappened();
        A.CallTo(() => clientB.GetDeviceAsync(DeviceB)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetDeviceAsync(DeviceA)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetDeviceAsync_ChannelAddress_UsesDeviceLevelRoute()
    {
        var clientA = CreateClientWithDevices(DeviceA);
        var clientB = CreateClientWithDevices(DeviceB);

        var channelDevice = A.Fake<ICcuDevice>();
        A.CallTo(() => channelDevice.Uri).Returns(new CcuDeviceUri
        {
            CcuHost = "ccu-b", Kind = CcuDeviceKind.HomeMatic, Address = $"{DeviceB}:1"
        });
        A.CallTo(() => clientB.GetDeviceAsync($"{DeviceB}:1")).Returns(Task.FromResult(channelDevice));
        A.CallTo(() => clientA.GetDeviceAsync($"{DeviceB}:1")).ThrowsAsync(new KeyNotFoundException());

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        // Populate routing for DeviceB via bulk call.
        _ = await multi.GetDevicesAsync();

        // Request a channel address "DeviceB:1"; routing should strip the channel suffix and go to clientB.
        await multi.GetDeviceAsync($"{DeviceB}:1");

        A.CallTo(() => clientA.GetDeviceAsync($"{DeviceB}:1")).MustNotHaveHappened();
        A.CallTo(() => clientB.GetDeviceAsync($"{DeviceB}:1")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetDeviceAsync_CachedClientFails_FallsBackToOtherClientsAndUpdatesRoute()
    {
        var clientA = CreateClientWithDevices(DeviceA);
        var clientB = CreateClientWithDevices(DeviceA);

        var routingTable = new CcuRoutingTable();
        // Pre-seed stale mapping: DeviceA owned by clientA even though it actually lives on clientB as well.
        // Make clientA fail to simulate a stale entry.
        A.CallTo(() => clientA.GetDeviceAsync(DeviceA)).ThrowsAsync(new KeyNotFoundException());
        routingTable.Register(DeviceA, clientA);

        var multi = new MultiCcuClient([clientA, clientB], routingTable);

        var device = await multi.GetDeviceAsync(DeviceA);

        device.Should().NotBeNull();
        A.CallTo(() => clientA.GetDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();

        routingTable.TryGetClient(DeviceA, out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(clientB);
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_SecondCall_UsesOnlyOwningClient()
    {
        var clientA = CreateClientWithCompleteDevices();
        var clientB = CreateClientWithCompleteDevices(DeviceA);

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        await multi.GetCompleteDeviceAsync(DeviceA);
        await multi.GetCompleteDeviceAsync(DeviceA);

        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceA)).MustHaveHappenedTwiceExactly();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCompleteDeviceAsync_NullOrWhitespaceAddress_ThrowsArgumentException(string? address)
    {
        var multi = new MultiCcuClient([CreateClientWithCompleteDevices()], new CcuRoutingTable());

        var act = () => multi.GetCompleteDeviceAsync(address!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_UnknownDevice_ProbesAllClientsAndThrows()
    {
        var clientA = CreateClientWithCompleteDevices();
        var clientB = CreateClientWithCompleteDevices();

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        var act = () => multi.GetCompleteDeviceAsync("UNKNOWN");

        await act.Should().ThrowAsync<KeyNotFoundException>();
        A.CallTo(() => clientA.GetCompleteDeviceAsync("UNKNOWN")).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetCompleteDeviceAsync("UNKNOWN")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_ChannelAddress_UsesDeviceLevelRoute()
    {
        var clientA = CreateClientWithCompleteDevices(DeviceA);
        var clientB = CreateClientWithCompleteDevices(DeviceB);

        var channelDevice = A.Fake<ICompleteCcuDevice>();
        A.CallTo(() => clientB.GetCompleteDeviceAsync($"{DeviceB}:1"))
            .Returns(Task.FromResult(channelDevice));
        A.CallTo(() => clientA.GetCompleteDeviceAsync($"{DeviceB}:1"))
            .ThrowsAsync(new KeyNotFoundException());

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        // Populate routing via the bulk call.
        _ = await multi.GetCompleteDevicesAsync();

        var result = await multi.GetCompleteDeviceAsync($"{DeviceB}:1");

        result.Should().BeSameAs(channelDevice);
        A.CallTo(() => clientA.GetCompleteDeviceAsync($"{DeviceB}:1")).MustNotHaveHappened();
        A.CallTo(() => clientB.GetCompleteDeviceAsync($"{DeviceB}:1")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_CachedClientFails_FallsBackToOtherClientsAndUpdatesRoute()
    {
        var clientA = CreateClientWithCompleteDevices(DeviceA);
        var clientB = CreateClientWithCompleteDevices(DeviceA);

        var routingTable = new CcuRoutingTable();
        // Pre-seed stale mapping to clientA, and make clientA throw to simulate stale entry.
        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).ThrowsAsync(new KeyNotFoundException());
        routingTable.Register(DeviceA, clientA);

        var multi = new MultiCcuClient([clientA, clientB], routingTable);

        var device = await multi.GetCompleteDeviceAsync(DeviceA);

        device.Should().NotBeNull();
        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();

        routingTable.TryGetClient(DeviceA, out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(clientB);
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_ReturnsAggregatedDevicesFromAllClients()
    {
        var clientA = CreateClientWithCompleteDevices(DeviceA);
        var clientB = CreateClientWithCompleteDevices(DeviceB);

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        var devices = (await multi.GetCompleteDevicesAsync()).ToList();

        devices.Should().HaveCount(2);
        devices.Select(d => d.DeviceData.Uri.Address).Should().BeEquivalentTo(DeviceA, DeviceB);
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_PopulatesRoutingTableSoSubsequentGetCompleteDeviceSkipsOtherClients()
    {
        var clientA = CreateClientWithCompleteDevices(DeviceA);
        var clientB = CreateClientWithCompleteDevices(DeviceB);

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        _ = await multi.GetCompleteDevicesAsync();

        await multi.GetCompleteDeviceAsync(DeviceA);
        await multi.GetCompleteDeviceAsync(DeviceB);

        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceB)).MustNotHaveHappened();
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceB)).MustHaveHappenedOnceExactly();
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceA)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_DeviceWithEmptyAddress_IsNotRegisteredInRoutingTable()
    {
        // One client returns a device with an empty address; the routing table must skip that entry.
        var clientA = A.Fake<ICcuClient>();
        var deviceData = A.Fake<ICcuDeviceData>();
        A.CallTo(() => deviceData.Uri).Returns(new CcuDeviceUri
        {
            CcuHost = "ccu", Kind = CcuDeviceKind.HomeMatic, Address = string.Empty
        });
        var device = A.Fake<ICompleteCcuDevice>();
        A.CallTo(() => device.DeviceData).Returns(deviceData);
        A.CallTo(() => clientA.GetCompleteDevicesAsync())
            .Returns(Task.FromResult<IEnumerable<ICompleteCcuDevice>>([device]));

        var routingTable = A.Fake<ICcuRoutingTable>();
        var multi = new MultiCcuClient([clientA], routingTable);

        var devices = (await multi.GetCompleteDevicesAsync()).ToList();

        devices.Should().ContainSingle().Which.Should().BeSameAs(device);
        A.CallTo(() => routingTable.Register(
                A<IEnumerable<KeyValuePair<string, ICcuClient>>>.That.Matches(entries => !entries.Any())))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompleteDevicesAsync_NoClients_ReturnsEmpty()
    {
        var multi = new MultiCcuClient([], new CcuRoutingTable());

        var devices = await multi.GetCompleteDevicesAsync();

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_NoClients_ThrowsKeyNotFoundException()
    {
        var multi = new MultiCcuClient([], new CcuRoutingTable());

        var act = () => multi.GetCompleteDeviceAsync(DeviceA);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_CachedClientThrowsNonKeyNotFound_PropagatesWithoutFallback()
    {
        // The cached client throws an unexpected exception. The fallback must not swallow it and
        // the other clients must not be probed.
        var clientA = CreateClientWithCompleteDevices();
        var clientB = CreateClientWithCompleteDevices(DeviceA);
        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).ThrowsAsync(new InvalidOperationException("net"));

        var routingTable = new CcuRoutingTable();
        routingTable.Register(DeviceA, clientA);

        var multi = new MultiCcuClient([clientA, clientB], routingTable);

        var act = () => multi.GetCompleteDeviceAsync(DeviceA);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("net");
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceA)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetCompleteDeviceAsync_ProbingClientThrowsNonKeyNotFound_Propagates()
    {
        // Only KeyNotFoundException should trigger the probe fallback; other exceptions must bubble up.
        var clientA = A.Fake<ICcuClient>();
        var clientB = CreateClientWithCompleteDevices(DeviceA);
        A.CallTo(() => clientA.GetCompleteDeviceAsync(DeviceA)).ThrowsAsync(new InvalidOperationException("net"));

        var multi = new MultiCcuClient([clientA, clientB], new CcuRoutingTable());

        var act = () => multi.GetCompleteDeviceAsync(DeviceA);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("net");
        A.CallTo(() => clientB.GetCompleteDeviceAsync(DeviceA)).MustNotHaveHappened();
    }

    private static ICcuClient CreateClientWithDevices(params string[] addresses)
    {
        var client = A.Fake<ICcuClient>();
        var devices = addresses.Select(address =>
        {
            var device = A.Fake<ICcuDevice>();
            A.CallTo(() => device.Uri).Returns(new CcuDeviceUri
            {
                CcuHost = "ccu", Kind = CcuDeviceKind.HomeMatic, Address = address
            });
            A.CallTo(() => client.GetDeviceAsync(address)).Returns(Task.FromResult(device));
            return device;
        }).ToArray();

        A.CallTo(() => client.GetDevicesAsync()).Returns(Task.FromResult(devices.AsEnumerable()));

        // Default: unknown addresses throw KeyNotFoundException.
        A.CallTo(() => client.GetDeviceAsync(A<string>.That.Matches(x => !addresses.Contains(x))))
            .ThrowsAsync(new KeyNotFoundException());

        return client;
    }

    private static ICcuClient CreateClientWithCompleteDevices(params string[] addresses)
    {
        var client = A.Fake<ICcuClient>();

        var devices = addresses.Select(address =>
        {
            var deviceData = A.Fake<ICcuDeviceData>();
            A.CallTo(() => deviceData.Uri).Returns(new CcuDeviceUri
            {
                CcuHost = "ccu", Kind = CcuDeviceKind.HomeMatic, Address = address
            });

            var device = A.Fake<ICompleteCcuDevice>();
            A.CallTo(() => device.DeviceData).Returns(deviceData);

            A.CallTo(() => client.GetCompleteDeviceAsync(address)).Returns(Task.FromResult(device));

            return device;
        }).ToArray();

        A.CallTo(() => client.GetCompleteDevicesAsync()).Returns(Task.FromResult(devices.AsEnumerable()));

        A.CallTo(() => client.GetCompleteDeviceAsync(A<string>.That.Matches(x => !addresses.Contains(x))))
            .ThrowsAsync(new KeyNotFoundException());

        return client;
    }
}
