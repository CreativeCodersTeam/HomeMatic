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
