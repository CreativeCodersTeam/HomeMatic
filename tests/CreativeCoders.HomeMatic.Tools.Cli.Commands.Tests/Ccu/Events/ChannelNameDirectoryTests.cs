using AwesomeAssertions;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class ChannelNameDirectoryTests
{
    private const string DeviceAddress = "000A1B2C3D4E5F";

    private const string ChannelAddress = "000A1B2C3D4E5F:1";

    [Fact]
    public void Lookup_ChannelAddressInDirectory_ReturnsDeviceName()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(ChannelAddress);

        // Assert
        name.Should().Be("Device name");
    }

    [Fact]
    public void Lookup_OnlyChannelAddressInDirectory_ReturnsNull()
    {
        // Arrange
        var sut = new ChannelNameDirectory(new Dictionary<string, string> { [ChannelAddress] = "Channel name" });

        // Act
        var name = sut.Lookup(ChannelAddress);

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void Lookup_OnlyDeviceAddressKnown_ReturnsDeviceName()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(DeviceAddress + ":2");

        // Assert
        name.Should().Be("Device name");
    }

    [Fact]
    public void Lookup_DeviceAddress_ReturnsDeviceName()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(DeviceAddress);

        // Assert
        name.Should().Be("Device name");
    }

    [Fact]
    public void Lookup_DifferentCase_ReturnsName()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(ChannelAddress.ToLowerInvariant());

        // Assert
        name.Should().Be("Device name");
    }

    [Theory]
    [InlineData("FFFFFFFFFFFFFF:1")]
    [InlineData("FFFFFFFFFFFFFF")]
    [InlineData(":1")]
    [InlineData("")]
    // Prefix of a known device address
    [InlineData("000A1B2C3D4E")]
    [InlineData("000A1B2C3D4E:1")]
    public void Lookup_UnknownAddress_ReturnsNull(string address)
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(address);

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void Unavailable_IsAvailable_IsFalse()
    {
        // Act
        var isAvailable = ChannelNameDirectory.Unavailable.IsAvailable;

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void Ctor_WithNames_IsAvailable()
    {
        // Act
        var sut = CreateDirectory();

        // Assert
        sut.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_DevicesWithChannels_ContainsDeviceNamesOnly()
    {
        // Arrange
        var client = CreateClient(
            new DeviceDetails
            {
                Address = DeviceAddress,
                Name = "Device name",
                Channels =
                [
                    new ChannelDetails { Address = ChannelAddress, Name = "Channel name" },
                    new ChannelDetails { Address = DeviceAddress + ":2", Name = "Second channel" }
                ]
            },
            new DeviceDetails { Address = "0011223344", Name = "Other device" });

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.IsAvailable.Should().BeTrue();
        directory.Lookup(DeviceAddress).Should().Be("Device name");
        directory.Lookup(ChannelAddress).Should().Be("Device name");
        directory.Lookup(DeviceAddress + ":2").Should().Be("Device name");
        directory.Lookup("0011223344:5").Should().Be("Other device");
    }

    [Fact]
    public async Task LoadAsync_EntriesWithMissingAddressOrName_SkipsThem()
    {
        // Arrange
        var client = CreateClient(
            new DeviceDetails
            {
                Address = DeviceAddress,
                Name = " ",
                Channels =
                [
                    new ChannelDetails { Address = ChannelAddress, Name = null },
                    new ChannelDetails { Address = null, Name = "No address" },
                    new ChannelDetails { Address = " ", Name = "Blank address" },
                    new ChannelDetails { Address = DeviceAddress + ":2", Name = "Second channel" }
                ]
            },
            new DeviceDetails { Address = null, Name = "Device without address" },
            new DeviceDetails { Address = " ", Name = "Device with blank address" },
            new DeviceDetails { Address = "0011223344", Name = "Other device" });

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.Lookup(DeviceAddress).Should().BeNull();
        directory.Lookup(ChannelAddress).Should().BeNull();
        directory.Lookup(" ").Should().BeNull();
        directory.Lookup(DeviceAddress + ":2").Should().BeNull();
        directory.Lookup("0011223344:1").Should().Be("Other device");
    }

    [Fact]
    public async Task LoadAsync_DuplicateAddresses_LastNameWins()
    {
        // Arrange
        var client = CreateClient(
            new DeviceDetails { Address = DeviceAddress, Name = "First" },
            new DeviceDetails { Address = DeviceAddress.ToLowerInvariant(), Name = "Second" });

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.Lookup(DeviceAddress).Should().Be("Second");
    }

    [Fact]
    public async Task LoadAsync_NoDevices_ReturnsAvailableEmptyDirectory()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.IsAvailable.Should().BeTrue();
        directory.Lookup(ChannelAddress).Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ListAllDetailsThrows_StillLogsOut()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.AutoLogout()).Returns(logout);
        A.CallTo(() => client.ListAllDetailsAsync()).ThrowsAsync(new HttpRequestException("CCU unreachable"));

        // Act
        await ChannelNameDirectory.LoadAsync(client);

        // Assert
        A.CallTo(() => logout.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task LoadAsync_LogoutThrowsAfterNamesLoaded_ReturnsLoadedDirectoryWithoutError()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        A.CallTo(() => logout.DisposeAsync()).Throws(new HttpRequestException("logout failed"));
        var client = CreateClient(new DeviceDetails { Address = DeviceAddress, Name = "Device name" });
        A.CallTo(() => client.AutoLogout()).Returns(logout);

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.IsAvailable.Should().BeTrue();
        directory.Lookup(DeviceAddress).Should().Be("Device name");
    }

    [Fact]
    public async Task LoadAsync_LogoutFaultsAsynchronouslyAfterNamesLoaded_ReturnsLoadedDirectoryWithoutError()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        A.CallTo(() => logout.DisposeAsync())
            .Returns(ValueTask.FromException(new HttpRequestException("logout failed")));
        var client = CreateClient(new DeviceDetails { Address = DeviceAddress, Name = "Device name" });
        A.CallTo(() => client.AutoLogout()).Returns(logout);

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        error.Should().BeNull();
        directory.Lookup(DeviceAddress).Should().Be("Device name");
    }

    [Fact]
    public async Task LoadAsync_ListAllDetailsAndLogoutThrow_ReturnsUnavailableWithLoadError()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        A.CallTo(() => logout.DisposeAsync()).Throws(new HttpRequestException("logout failed"));
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.AutoLogout()).Returns(logout);
        A.CallTo(() => client.ListAllDetailsAsync()).ThrowsAsync(new HttpRequestException("CCU unreachable"));

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("CCU unreachable");
    }

    [Fact]
    public async Task LoadAsync_AutoLogoutThrows_ReturnsUnavailableWithError()
    {
        // Arrange
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.AutoLogout()).Throws(new InvalidOperationException("no session"));

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("no session");
        A.CallTo(() => client.ListAllDetailsAsync()).MustNotHaveHappened();
    }

    [Fact]
    public void Ctor_KeysDifferingOnlyInCase_LastNameWins()
    {
        // Arrange
        var names = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DeviceAddress] = "Upper",
            [DeviceAddress.ToLowerInvariant()] = "Lower"
        };

        // Act
        var sut = new ChannelNameDirectory(names);

        // Assert
        sut.Lookup(ChannelAddress).Should().Be("Lower");
    }

    [Fact]
    public void Lookup_NullAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var act = () => sut.Lookup(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Lookup_Unavailable_ReturnsNull()
    {
        // Act
        var name = ChannelNameDirectory.Unavailable.Lookup(ChannelAddress);

        // Assert
        name.Should().BeNull();
    }

    [Fact]
    public void Lookup_AddressWithSeveralSeparators_FallsBackToPartBeforeFirstSeparator()
    {
        // Arrange
        var sut = CreateDirectory();

        // Act
        var name = sut.Lookup(DeviceAddress + ":1:2");

        // Assert
        name.Should().Be("Device name");
    }

    [Fact]
    public async Task LoadAsync_ListAllDetailsThrows_ReturnsUnavailableWithErrorMessage()
    {
        // Arrange
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync()).ThrowsAsync(new HttpRequestException("CCU unreachable"));

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client);

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("CCU unreachable");
    }

    [Fact]
    public async Task LoadAsync_Loaded_LogsOut()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        var client = CreateClient(new DeviceDetails { Address = DeviceAddress, Name = "Device name" });
        A.CallTo(() => client.AutoLogout()).Returns(logout);

        // Act
        await ChannelNameDirectory.LoadAsync(client);

        // Assert
        A.CallTo(() => logout.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_LoadedInTime_ReturnsLoadedDirectory()
    {
        // Arrange
        var client = CreateClient(new DeviceDetails { Address = DeviceAddress, Name = "Device name" });

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client, TimeSpan.FromSeconds(10));

        // Assert
        error.Should().BeNull();
        directory.IsAvailable.Should().BeTrue();
        directory.Lookup(DeviceAddress).Should().Be("Device name");
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_LoadFails_ReturnsUnavailableWithErrorMessage()
    {
        // Arrange
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync()).ThrowsAsync(new HttpRequestException("CCU unreachable"));

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client, TimeSpan.FromSeconds(10));

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("CCU unreachable");
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_LoadNeverCompletes_ReturnsUnavailableWithTimeoutError()
    {
        // Arrange
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync())
            .Returns(new TaskCompletionSource<IEnumerable<DeviceDetails>>().Task);

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client, TimeSpan.FromMilliseconds(100))
            .WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("timeout");
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_CancelledWhileLoading_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync())
            .ReturnsLazily(() =>
            {
                cancellationTokenSource.Cancel();
                return new TaskCompletionSource<IEnumerable<DeviceDetails>>().Task;
            });

        // Act
        var act = () => ChannelNameDirectory
            .LoadAsync(client, TimeSpan.FromSeconds(10), cancellationTokenSource.Token)
            .WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_AlreadyCancelled_ThrowsWithoutQueryingCcu()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = () => ChannelNameDirectory.LoadAsync(client, TimeSpan.FromSeconds(10), new CancellationToken(true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        A.CallTo(() => client.ListAllDetailsAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_NullClient_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ChannelNameDirectory.LoadAsync(null!, TimeSpan.FromSeconds(10));

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_LoadCompletesAfterTimeout_StillLogsOut()
    {
        // Arrange
        var logout = A.Fake<IAsyncDisposable>();
        var pendingDevices = new TaskCompletionSource<IEnumerable<DeviceDetails>>();
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.AutoLogout()).Returns(logout);
        A.CallTo(() => client.ListAllDetailsAsync()).Returns(pendingDevices.Task);
        var (_, error) = await ChannelNameDirectory.LoadAsync(client, TimeSpan.FromMilliseconds(50))
            .WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        pendingDevices.SetResult([]);

        // Assert
        error.Should().Be("timeout");
        A.CallTo(() => logout.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task LoadAsyncWithTimeout_ClientThrowsOperationCanceledWithoutCancellation_ReturnsUnavailable()
    {
        // Arrange
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync()).ThrowsAsync(new TaskCanceledException("HTTP timeout"));

        // Act
        var (directory, error) = await ChannelNameDirectory.LoadAsync(client, TimeSpan.FromSeconds(10));

        // Assert
        directory.Should().BeSameAs(ChannelNameDirectory.Unavailable);
        error.Should().Be("HTTP timeout");
    }

    private static ChannelNameDirectory CreateDirectory()
    {
        return new ChannelNameDirectory(new Dictionary<string, string>
        {
            [DeviceAddress] = "Device name",
            [ChannelAddress] = "Channel name"
        });
    }

    private static IHomeMaticJsonRpcClient CreateClient(params DeviceDetails[] devices)
    {
        var client = A.Fake<IHomeMaticJsonRpcClient>();
        A.CallTo(() => client.ListAllDetailsAsync()).Returns(devices);

        return client;
    }
}
