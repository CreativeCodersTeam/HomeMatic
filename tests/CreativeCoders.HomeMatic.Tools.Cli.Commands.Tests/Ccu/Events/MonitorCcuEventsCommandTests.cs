using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Models;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Commanding;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Connections;
using CreativeCoders.HomeMatic.Tools.Cli.Base.Events;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using FakeItEasy;
using Spectre.Console;
using Spectre.Console.Testing;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

[SuppressMessage("csharpsquid", "S6608:Prefer indexing instead of \"Enumerable\" methods on types implementing \"IList\"",
    Justification = "Performance rule is not relevant for tests.")]
public class MonitorCcuEventsCommandTests
{
    private const string ConnectionName = "test-ccu";

    private const int CallbackPort = 53817;

    private static readonly Uri CcuUrl = new("http://ccu.example.local/");

    private static readonly CallbackEndpoint Endpoint = new("192.168.1.20", CallbackPort);

    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task ExecuteAsync_UnknownConnectionName_PrintsNotFoundAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = "x" });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("CCU connection 'x' not found");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.EventServerFactory.Create(A<string>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    public async Task ExecuteAsync_CallbackPortOutOfRange_PrintsInvalidPortAndReturnsError(int callbackPort)
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EndpointResolver.Resolve(CcuUrl, null, callbackPort))
            .Throws(new ArgumentOutOfRangeException("callbackPort", callbackPort, "Port out of range"));

        // Act
        var result = await sut.Command.ExecuteAsync(
            new MonitorCcuEventsOptions { Name = ConnectionName, CallbackPort = callbackPort });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("--callback-port must be between 0 and 65535");
        A.CallTo(() => sut.EventServerFactory.Create(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_AllInitCallsThrow_PrintsNoInterfaceAndReturnsErrorAndDisposesServer()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain(
            "No CCU interface could be subscribed. Is the CCU reachable at http://ccu.example.local/?");
        sut.OutputText.Should().Contain(
            "Interface BidCos-RF not available: connection failed on port 2001 (Connection refused)");
        sut.OutputText.Should().Contain(
            "Interface HmIP-RF not available: connection failed on port 2010 (Connection refused)");
        sut.OutputText.Should().Contain(
            "Interface BidCos-Wired not available: connection failed on port 2000 (Connection refused)");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, A<string>._))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_KnownConnectionName_UsesConnectionAndStartsServerOnListenPrefix()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions
        {
            Name = ConnectionName,
            CallbackHost = "my-host",
            CallbackPort = 1234
        });

        // Assert
        A.CallTo(() => sut.EndpointResolver.Resolve(CcuUrl, "my-host", 1234)).MustHaveHappenedOnceExactly();
        A.CallTo(() => sut.EventServerFactory.Create(Endpoint.ListenPrefix)).MustHaveHappenedOnceExactly();
        A.CallTo(() => sut.EventServer.RegisterEventHandler(A<CcuEventChannelHandler>._))
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => sut.EventServer.StartAsync()).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustHaveHappened());
    }

    [Fact]
    public async Task ExecuteAsync_StartThrowsAddressInUse_PrintsPortInUseAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EventServer.StartAsync())
            .ThrowsAsync(StartException(CcuEventServerStartFailure.AddressInUse, "Address already in use"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain(
            $"Callback port {CallbackPort} is already in use. Use --callback-port to choose another port.");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_StartThrowsAccessDenied_PrintsPlatformHintAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EventServer.StartAsync())
            .ThrowsAsync(StartException(CcuEventServerStartFailure.AccessDenied, "Access is denied"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain(OperatingSystem.IsWindows()
            ? $"Not allowed to listen on port {CallbackPort}. Run as administrator or add a URL ACL: " +
              $"netsh http add urlacl url=http://+:{CallbackPort}/ user="
            : $"Not allowed to listen on port {CallbackPort}. Use --callback-port to choose a port above 1023.");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_StartThrowsOtherListenerError_PrintsListenErrorAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EventServer.StartAsync())
            .ThrowsAsync(StartException(CcuEventServerStartFailure.Other, "Prefix [conflict]"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain($"Cannot listen on callback port {CallbackPort}: Prefix [conflict]");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_StartThrowsUnexpectedException_PropagatesAndDisposesServer()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EventServer.StartAsync()).ThrowsAsync(new InvalidOperationException("broken"));

        // Act
        var act = () => sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("broken");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    public async Task ExecuteAsync_CallbackPortAtBoundary_PassesPortToResolver(int callbackPort)
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await sut.Command.ExecuteAsync(
            new MonitorCcuEventsOptions { Name = ConnectionName, CallbackPort = callbackPort });

        // Assert
        sut.OutputText.Should().NotContain("--callback-port must be between");
        A.CallTo(() => sut.EndpointResolver.Resolve(CcuUrl, A<string?>._, callbackPort))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_GetCredentialsThrows_PrintsDeviceNamesUnavailableAndContinues()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.ConnectionsStore.GetCredentials(A<CcuConnectionInfo>._))
            .Throws(new InvalidOperationException("No credentials"));
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("Device names unavailable: No credentials. Showing addresses only.");
        A.CallTo(() => sut.EventServer.StartAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_EndpointCannotBeResolved_PrintsHintAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EndpointResolver.Resolve(A<Uri>._, A<string?>._, A<int>._))
            .Throws(new SocketException((int)SocketError.HostNotFound));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("Cannot determine the callback address for ccu.example.local:");
        sut.OutputText.Should().Contain("Use --callback-host to set it.");
        A.CallTo(() => sut.EventServerFactory.Create(A<string>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_CallbackPortCannotBeAllocated_PrintsPortHintAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.EndpointResolver.Resolve(A<Uri>._, A<string?>._, A<int>._))
            .Throws(new CallbackPortAllocationException("No free port",
                new SocketException((int)SocketError.AddressAlreadyInUse)));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("Cannot allocate a callback port: No free port.");
        sut.OutputText.Should().Contain("Use --callback-port to choose a port.");
        sut.OutputText.Should().NotContain("Cannot determine the callback address");
        A.CallTo(() => sut.EventServerFactory.Create(A<string>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_DeviceNamesFailAndAllInitCallsThrow_PrintsBothMessagesAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.JsonRpcClient.ListAllDetailsAsync())
            .ThrowsAsync(new HttpRequestException("JSON-RPC down"));
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("Device names unavailable: JSON-RPC down. Showing addresses only.");
        sut.OutputText.Should().Contain("No CCU interface could be subscribed");
        A.CallTo(() => sut.EventServer.StartAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WarningsWithMarkupCharacters_PrintsThemLiterally()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.JsonRpcClient.ListAllDetailsAsync())
            .ThrowsAsync(new HttpRequestException("Connection refused [::1]:80"));
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused [::1]:2001"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain(
            "Device names unavailable: Connection refused [::1]:80. Showing addresses only.");
        sut.OutputText.Should().Contain(
            "Interface BidCos-RF not available: connection failed on port 2001 (Connection refused [::1]:2001)");
    }

    [Fact]
    public async Task ExecuteAsync_ConnectionFound_LoadsNamesWithStoredCredentials()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        A.CallTo(() => sut.JsonRpcClientBuilder.ForUrl(CcuUrl)).MustHaveHappenedOnceExactly();
        A.CallTo(() => sut.JsonRpcClientBuilder.WithCredentials(sut.Credential)).MustHaveHappenedOnceExactly();
        A.CallTo(() => sut.JsonRpcClient.ListAllDetailsAsync()).MustHaveHappenedOnceExactly();
        sut.OutputText.Should().NotContain("Device names unavailable");
    }

    [Fact]
    public async Task ExecuteAsync_CancelKeyAfterStartup_UnsubscribesDisposesServerAndReturnsSuccess()
    {
        // Arrange
        var sut = new SutContext();

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.TakeLast(2).Should().Equal("Stopping ...", "Event monitor stopped.");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(3, Times.Exactly)
            .Then(A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly());
        A.CallTo(() => sut.CancelKeyRegistration.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_UnsubscribeThrows_PrintsWarningPerInterfaceAndReturnsSuccess()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Connection reset"));

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.TakeLast(5).Should().Equal(
            "Stopping ...",
            "Unsubscribe from BidCos-RF failed: connection failed on port 2001 (Connection reset)",
            "Unsubscribe from HmIP-RF failed: connection failed on port 2010 (Connection reset)",
            "Unsubscribe from BidCos-Wired failed: connection failed on port 2000 (Connection reset)",
            "Event monitor stopped.");
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_UnsubscribeHangs_StopsWithinTimeoutAndPrintsTimeoutWarning()
    {
        // Arrange
        var sut = new SutContext();
        var neverCompletes = new TaskCompletionSource();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, string.Empty)).Returns(neverCompletes.Task);

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().Contain("Unsubscribe from HmIP-RF failed: timeout");
        sut.OutputLines[^1].Should().Be("Event monitor stopped.");
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CancelKeyDuringSubscribe_SkipsReadLoopAndUnsubscribesAllSubscribedKinds()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .ReturnsLazily(() =>
            {
                sut.RaiseCancel();
                return Task.CompletedTask;
            });

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().NotContain("Press Ctrl+C, Q or Esc to stop.");
        sut.OutputLines.TakeLast(2).Should().Equal("Stopping ...", "Event monitor stopped.");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CancelKeyDuringSubscribeWithOneFailedKind_UnsubscribesOnlySubscribedKinds()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .ReturnsLazily(() =>
            {
                sut.RaiseCancel();
                return Task.CompletedTask;
            });
        A.CallTo(() => sut.XmlRpcApi.InitAsync(
                A<string>._, A<string>.That.EndsWith($"-{CcuDeviceKind.HomeMaticWired}")))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().Contain(
            "Interface BidCos-Wired not available: connection failed on port 2000 (Connection refused)");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_EventsReceivedWhileRunning_PrintedBeforeStopping()
    {
        // Arrange
        var sut = new SutContext();

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        await sut.EventHandler!.Event(
            sut.InterfaceIdOf(CcuDeviceKind.HomeMaticIp), "000A1B2C3D4E5F:1", "STATE", true);
        await sut.WaitForOutputAsync("STATE = true");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        var lines = sut.OutputLines;
        Array.FindIndex(lines, x => x.Contains("HmIP-RF") && x.EndsWith("STATE = true"))
            .Should().BeLessThan(Array.IndexOf(lines, "Stopping ..."));
    }

    [Fact]
    public async Task ExecuteAsync_EventsQueuedWhenStopped_PrintedBeforeStopping()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(
                A<string>._, A<string>.That.EndsWith($"-{CcuDeviceKind.HomeMaticWired}")))
            .ReturnsLazily((string callbackUrl, string interfaceId) =>
            {
                _ = sut.EventHandler!.Event(interfaceId, "0011223344:1", "STATE", false);
                _ = sut.EventHandler.Event(interfaceId, "0011223344:2", "LEVEL", 0.5);
                sut.RaiseCancel();
                return Task.CompletedTask;
            });

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.TakeLast(4).Select(x => x.Split("  ", StringSplitOptions.RemoveEmptyEntries).Last())
            .Should().Equal("STATE = false", "LEVEL = 0.5", "Stopping ...", "Event monitor stopped.");
    }

    [Fact]
    public async Task ExecuteAsync_UnknownConnectionName_ReturnsBeforeHandlingStopKeys()
    {
        // Arrange
        var sut = new SutContext(interactive: true);

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = "x" });

        // Assert
        result.ExitCode.Should().Be(-1);
        A.CallTo(() => sut.CancelKeySource.Register(A<Action>._)).MustNotHaveHappened();
        Fake.GetCalls(sut.Input).Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_NoInterfaceSubscribed_DisposesCancelKeyRegistration()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        A.CallTo(() => sut.CancelKeySource.Register(A<Action>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => sut.CancelKeyRegistration.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_NoInterfaceSubscribed_PrintsNoStopMessages()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().NotContain("Stopping ...");
        sut.OutputText.Should().NotContain("Event monitor stopped.");
        A.CallTo(() => sut.CancelKeyRegistration.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(ConsoleKey.Q)]
    [InlineData(ConsoleKey.Escape)]
    public async Task ExecuteAsync_StopKeyPressedAfterStartup_UnsubscribesAndReturnsSuccess(ConsoleKey key)
    {
        // Arrange
        var sut = new SutContext(interactive: true);

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.PressKey(ConsoleKey.A);
        sut.PressKey(key);
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.TakeLast(2).Should().Equal("Stopping ...", "Event monitor stopped.");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_InteractiveConsoleAndNoInterfaceSubscribed_ReturnsErrorWithoutWaitingForKey()
    {
        // Arrange
        var sut = new SutContext(interactive: true);
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(-1);
        A.CallTo(() => sut.Input.IsKeyAvailable()).MustHaveHappened();
        A.CallTo(() => sut.CancelKeyRegistration.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_KeyInputFails_CancelKeyStillStopsAndReturnsSuccess()
    {
        // Arrange
        var sut = new SutContext(interactive: true);
        A.CallTo(() => sut.Input.IsKeyAvailable()).Throws(new InvalidOperationException("input redirected"));

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines[^1].Should().Be("Event monitor stopped.");
    }

    [Fact]
    public async Task ExecuteAsync_CancelKeyPressedTwice_RunsStopFlowOnce()
    {
        // Arrange
        var sut = new SutContext();

        // The second key press happens while the stop flow is still running (during unsubscribing).
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .Invokes(() => sut.RaiseCancel());

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.Count(x => x == "Stopping ...").Should().Be(1);
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public async Task ExecuteAsync_CancelKeyDuringNameLoading_StopsWithoutSubscribingAndReturnsSuccess()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.JsonRpcClient.ListAllDetailsAsync())
            .ReturnsLazily(() =>
            {
                sut.RaiseCancel();
                return new TaskCompletionSource<IEnumerable<DeviceDetails>>().Task;
            });

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().NotContain("Device names unavailable");
        sut.OutputLines.Should().Equal("Stopping ...", "Event monitor stopped.");
        A.CallTo(() => sut.EventServerFactory.Create(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.CancelKeyRegistration.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_StopKeyDuringNameLoading_StopsWithoutSubscribingAndReturnsSuccess()
    {
        // Arrange
        var sut = new SutContext(interactive: true);
        A.CallTo(() => sut.JsonRpcClient.ListAllDetailsAsync())
            .ReturnsLazily(() =>
            {
                sut.PressKey(ConsoleKey.Q);
                return new TaskCompletionSource<IEnumerable<DeviceDetails>>().Task;
            });

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        A.CallTo(() => sut.EventServerFactory.Create(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_AllSubscribesTimeOut_UnsubscribesTimedOutKindsAndReturnsError()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .Returns(new TaskCompletionSource().Task);

        // Act
        var result = await sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName })
            .WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(-1);
        sut.OutputText.Should().Contain("Interface HmIP-RF not available: timeout");
        sut.OutputLines[^1].Should().Be(
            "No CCU interface could be subscribed. Is the CCU reachable at http://ccu.example.local/?");
        sut.OutputText.Should().NotContain("Stopping ...");
        A.CallTo(() => sut.XmlRpcApi.InitAsync(Endpoint.CallbackUrl, string.Empty))
            .MustHaveHappened(3, Times.Exactly)
            .Then(A.CallTo(() => sut.EventServer.DisposeAsync()).MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task ExecuteAsync_AddressFilter_PrintsOnlyEventsOfMatchingDevice()
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions { Name = ConnectionName, Addresses = ["000A1B2C3D4E5F"] };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        var interfaceId = sut.InterfaceIdOf(CcuDeviceKind.HomeMaticIp);
        await sut.EventHandler!.Event(interfaceId, "0011223344:1", "STATE", true);
        await sut.EventHandler.Event(interfaceId, "000A1B2C3D4E5F:1", "STATE", false);
        await sut.EventHandler.Event(interfaceId, "0011223344:2", "LEVEL", 0.5);
        await sut.EventHandler.Event(interfaceId, "000A1B2C3D4E5F:2", "LEVEL", 1.0);
        await sut.WaitForOutputAsync("LEVEL = 1");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().NotContain("0011223344");
        var eventLines = sut.OutputLines.Where(x => x.Contains(" = ")).ToArray();
        eventLines.Should().HaveCount(2);
        eventLines[0].Should().Contain("  000A1B2C3D4E5F:1  ").And.EndWith("STATE = false");
        eventLines[1].Should().Contain("  000A1B2C3D4E5F:2  ").And.EndWith("LEVEL = 1");
    }

    [Fact]
    public async Task ExecuteAsync_AddressAndValueKeyFilter_PrintsOnlyEventsMatchingBoth()
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions
        {
            Name = ConnectionName,
            Addresses = ["000A1B2C3D4E5F"],
            ValueKeys = ["state"]
        };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        var interfaceId = sut.InterfaceIdOf(CcuDeviceKind.HomeMatic);
        await sut.EventHandler!.Event(interfaceId, "000A1B2C3D4E5F:1", "LEVEL", 0.5);
        await sut.EventHandler.Event(interfaceId, "0011223344:1", "STATE", true);
        await sut.EventHandler.Event(interfaceId, "000A1B2C3D4E5F:1", "STATE", false);
        await sut.WaitForOutputAsync("STATE = false");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.Where(x => x.Contains(" = ")).Should().ContainSingle()
            .Which.Should().EndWith("000A1B2C3D4E5F:1  <unknown>  STATE = false");
    }

    [Fact]
    public async Task ExecuteAsync_AddressFilterAndEventsQueuedWhenStopped_DrainPrintsOnlyMatchingEvents()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(
                A<string>._, A<string>.That.EndsWith($"-{CcuDeviceKind.HomeMaticWired}")))
            .ReturnsLazily((string callbackUrl, string interfaceId) =>
            {
                _ = sut.EventHandler!.Event(interfaceId, "0011223344:1", "STATE", false);
                _ = sut.EventHandler.Event(interfaceId, "000A1B2C3D4E5F:2", "LEVEL", 0.5);
                _ = sut.EventHandler.Event(interfaceId, "0011223344:2", "STATE", true);
                sut.RaiseCancel();
                return Task.CompletedTask;
            });
        var options = new MonitorCcuEventsOptions { Name = ConnectionName, Addresses = ["000A1B2C3D4E5F"] };

        // Act
        var result = await sut.Command.ExecuteAsync(options).WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().NotContain("0011223344");
        sut.OutputLines.TakeLast(3).Select(x => x.Split("  ", StringSplitOptions.RemoveEmptyEntries).Last())
            .Should().Equal("LEVEL = 0.5", "Stopping ...", "Event monitor stopped.");
    }

    [Fact]
    public async Task ExecuteAsync_AddressAndValueKeyFilter_PrintsFilterLineInStartupBlock()
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions
        {
            Name = ConnectionName,
            Addresses = ["000A1B2C3D4E5F"],
            ValueKeys = ["STATE"]
        };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        await run.WaitAsync(TestTimeout);

        // Assert
        var lines = sut.OutputLines;
        var filterIndex = Array.IndexOf(lines, "  Filter     : address 000A1B2C3D4E5F | value key STATE");
        filterIndex.Should().Be(Array.IndexOf(lines, $"  Callback   : {Endpoint.CallbackUrl}") + 1);
        lines[filterIndex + 1].Should().Be("  Press Ctrl+C, Q or Esc to stop.");
    }

    [Theory]
    [InlineData(new[] { " 000a1b2c3d4e5f ", "", "0011223344:1" }, null,
        "  Filter     : address 000a1b2c3d4e5f, 0011223344:1")]
    [InlineData(null, new[] { "state", "LEVEL" }, "  Filter     : value key state, LEVEL")]
    public async Task ExecuteAsync_OneFilterSet_PrintsOnlyThatFilterInStartupBlock(
        string[]? addresses,
        string[]? valueKeys,
        string expectedLine)
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions
        {
            Name = ConnectionName,
            Addresses = addresses,
            ValueKeys = valueKeys
        };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        await run.WaitAsync(TestTimeout);

        // Assert
        sut.OutputLines.Should().Contain(expectedLine);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" , ")]
    public async Task ExecuteAsync_NoFilterSet_PrintsNoFilterLine(string? addresses)
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions { Name = ConnectionName, Addresses = addresses?.Split(',') };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputText.Should().NotContain("Filter");
    }

    [Fact]
    public async Task ExecuteAsync_ValueKeyFilter_PrintsOnlyMatchingValueKeysOfAnyAddress()
    {
        // Arrange
        var sut = new SutContext();
        var options = new MonitorCcuEventsOptions { Name = ConnectionName, ValueKeys = ["level"] };

        // Act
        var run = sut.Command.ExecuteAsync(options);
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        var interfaceId = sut.InterfaceIdOf(CcuDeviceKind.HomeMaticIp);
        await sut.EventHandler!.Event(interfaceId, "000A1B2C3D4E5F:1", "STATE", true);
        await sut.EventHandler.Event(interfaceId, "000A1B2C3D4E5F:1", "LEVEL", 0.25);
        await sut.EventHandler.Event(interfaceId, "0011223344:2", "LEVEL", 0.75);
        await sut.WaitForOutputAsync("LEVEL = 0.75");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines
            .Where(x => x.Contains(" = "))
            .Select(x => x[x.LastIndexOf("  ", StringComparison.Ordinal)..].Trim())
            .Should().Equal("LEVEL = 0.25", "LEVEL = 0.75");
    }

    [Fact]
    public async Task ExecuteAsync_CredentialsPrompt_RunsBeforeCtrlCAndStopKeysAreHandled()
    {
        // Arrange
        var sut = new SutContext(interactive: true);
        var cancelKeyRegisteredDuringPrompt = true;
        var keysReadDuringPrompt = true;
        A.CallTo(() => sut.ConnectionsStore.GetCredentials(A<CcuConnectionInfo>._))
            .ReturnsLazily(() =>
            {
                cancelKeyRegisteredDuringPrompt = Fake.GetCalls(sut.CancelKeySource).Any();
                keysReadDuringPrompt = Fake.GetCalls(sut.Input).Any();
                return sut.Credential;
            });

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        cancelKeyRegisteredDuringPrompt.Should().BeFalse();
        keysReadDuringPrompt.Should().BeFalse();
        A.CallTo(() => sut.ConnectionsStore.GetCredentials(A<CcuConnectionInfo>._)).MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => sut.CancelKeySource.Register(A<Action>._)).MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task ExecuteAsync_RedirectedOutputAndEventLongerThanWidth_PrintsEventAsSingleLine()
    {
        // Arrange
        const string value = "ccu3-og-BidCos-RF#07.10.2026 08:38:03.659766 [test]";
        var sut = new SutContext(width: 80);

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        await sut.EventHandler!.Event(sut.InterfaceIdOf(CcuDeviceKind.HomeMatic), "CENTRAL", "PONG", value);
        await sut.WaitForOutputAsync("[test]");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        var lines = sut.OutputLines;
        lines.Should().Contain("Listening for events of CCU 'test-ccu' (http://ccu.example.local/)");
        lines.Should().Contain("  Press Ctrl+C, Q or Esc to stop.");
        var eventLine = lines.Should().ContainSingle(x => x.Contains("PONG")).Which;
        eventLine.Should().MatchRegex(@"^\d{2}:\d{2}:\d{2}\.\d{3}  BidCos-RF     CENTRAL  <unknown>  PONG = ")
            .And.EndWith($"PONG = \"{value}\"");
        eventLine.Length.Should().BeGreaterThan(80);
        lines.TakeLast(3).Should().Equal(eventLine, "Stopping ...", "Event monitor stopped.");
    }

    [Fact]
    public async Task ExecuteAsync_TerminalOutputAndEventLongerThanWidth_PrintsEventThroughConsoleRenderer()
    {
        // Arrange
        const string value = "ccu3-og-BidCos-RF#07.10.2026 08:38:03.659766 [test]";
        var sut = new SutContext(width: 80, terminal: true);

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        await sut.EventHandler!.Event(sut.InterfaceIdOf(CcuDeviceKind.HomeMatic), "CENTRAL", "PONG", value);
        await sut.WaitForOutputAsync("[test]");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        var lines = sut.OutputLines;
        var eventIndex = Array.FindIndex(lines, x => x.Contains("PONG"));
        lines[eventIndex].Length.Should().BeLessThanOrEqualTo(80);
        string.Concat(lines[eventIndex], lines[eventIndex + 1]).Should().EndWith($"PONG = \"{value}\"");
    }

    [Fact]
    public async Task ExecuteAsync_InterfaceReturnsNoXmlRpcResponse_PrintsNoXmlRpcServiceWarning()
    {
        // Arrange
        var sut = new SutContext();
        A.CallTo(() => sut.XmlRpcApi.InitAsync(
                A<string>._, A<string>.That.EndsWith($"-{CcuDeviceKind.HomeMaticWired}")))
            .ThrowsAsync(new XmlException(
                "For security reasons DTD is prohibited in this XML document. To enable DTD processing set the " +
                "DtdProcessing property on XmlReaderSettings to Parse and pass the settings into XmlReader.Create " +
                "method."));

        // Act
        var run = sut.Command.ExecuteAsync(new MonitorCcuEventsOptions { Name = ConnectionName });
        await sut.WaitForOutputAsync("Press Ctrl+C, Q or Esc to stop.");
        sut.RaiseCancel();
        var result = await run.WaitAsync(TestTimeout);

        // Assert
        result.ExitCode.Should().Be(0);
        sut.OutputLines.Should().Contain("Interface BidCos-Wired not available: no XML-RPC service on port 2000");
        sut.OutputText.Should().NotContain("DTD");
    }

    private static CcuEventServerStartException StartException(CcuEventServerStartFailure reason, string message)
    {
        return new CcuEventServerStartException(reason, message, new InvalidOperationException(message));
    }

    private sealed class SutContext
    {
        private readonly LockedTextWriter _output = new();

        private readonly ConcurrentQueue<ConsoleKeyInfo> _pressedKeys = new();

        private Action? _onCancel;

        public SutContext(bool interactive = false, int width = 500, bool terminal = false)
        {
            IAnsiConsoleOutput output = new AnsiConsoleOutput(_output);

            if (terminal)
            {
                // AnsiConsoleOutput only reports a terminal for the real stdout/stderr, so the output is faked.
                output = A.Fake<IAnsiConsoleOutput>();
                A.CallTo(() => output.Writer).Returns(_output);
                A.CallTo(() => output.IsTerminal).Returns(true);
                A.CallTo(() => output.Width).Returns(width);
                A.CallTo(() => output.Height).Returns(24);
            }

            // The thread-safe writer replaces the StringWriter of the TestConsole, because the tests read the output
            // while the command writes it. Setting a terminal output resets the width, so the width is set afterwards.
            var testConsole = new TestConsole();
            testConsole.Profile.Out = output;
            testConsole.Width(width);

            IAnsiConsole console = testConsole;

            if (interactive)
            {
                // Writes go to the test console; only the key input is faked.
                testConsole.Interactive();
                console = A.Fake<IAnsiConsole>(x => x.Wrapping(testConsole));
                A.CallTo(() => console.Input).Returns(Input);
                A.CallTo(() => Input.IsKeyAvailable()).ReturnsLazily(() => !_pressedKeys.IsEmpty);
                A.CallTo(() => Input.ReadKey(true))
                    .ReturnsLazily(() => _pressedKeys.TryDequeue(out var key) ? key : null);
            }

            var connection = new CcuConnectionInfo(CcuUrl, ConnectionName);

            A.CallTo(() => ConnectionsStore.FindConnectionAsync(A<string>._)).Returns((CcuConnectionInfo?)null);
            A.CallTo(() => ConnectionsStore.FindConnectionAsync(ConnectionName)).Returns(connection);
            A.CallTo(() => ConnectionsStore.GetCredentials(connection)).Returns(Credential);

            var xmlRpcApiBuilder = A.Fake<IHomeMaticXmlRpcApiBuilder>();
            A.CallTo(() => xmlRpcApiBuilder.ForUrl(A<XmlRpcApiAddress>._)).Returns(xmlRpcApiBuilder);
            A.CallTo(() => xmlRpcApiBuilder.Build()).Returns(XmlRpcApi);
            A.CallTo(() => XmlRpcApi.InitAsync(A<string>._, A<string>._))
                .Invokes((string _, string interfaceId) => InterfaceIds.Enqueue(interfaceId))
                .Returns(Task.CompletedTask);

            A.CallTo(() => JsonRpcClientBuilder.ForUrl(A<Uri>._)).Returns(JsonRpcClientBuilder);
            A.CallTo(() => JsonRpcClientBuilder.WithCredentials(A<NetworkCredential>._)).Returns(JsonRpcClientBuilder);
            A.CallTo(() => JsonRpcClientBuilder.Build()).Returns(JsonRpcClient);
            A.CallTo(() => JsonRpcClient.ListAllDetailsAsync()).Returns([]);

            A.CallTo(() => EventServerFactory.Create(A<string>._)).Returns(EventServer);
            A.CallTo(() => EventServer.RegisterEventHandler(A<ICcuEventHandler>._))
                .Invokes((ICcuEventHandler handler) => EventHandler = handler);

            A.CallTo(() => EndpointResolver.Resolve(A<Uri>._, A<string?>._, A<int>._)).Returns(Endpoint);

            A.CallTo(() => CancelKeySource.Register(A<Action>._))
                .ReturnsLazily((Action onCancel) =>
                {
                    _onCancel = onCancel;
                    return CancelKeyRegistration;
                });

            Command = new MonitorCcuEventsCommand(console, ConnectionsStore, xmlRpcApiBuilder,
                JsonRpcClientBuilder, EventServerFactory, EndpointResolver, CancelKeySource);
        }

        public void RaiseCancel()
        {
            (_onCancel ?? throw new InvalidOperationException("No cancel handler registered")).Invoke();
        }

        public void PressKey(ConsoleKey key)
        {
            _pressedKeys.Enqueue(new ConsoleKeyInfo('\0', key, false, false, false));
        }

        public string InterfaceIdOf(CcuDeviceKind kind)
        {
            return InterfaceIds.Single(x => x.EndsWith($"-{kind}", StringComparison.Ordinal));
        }

        public async Task WaitForOutputAsync(string text)
        {
            using var timeout = new CancellationTokenSource(TestTimeout);

            while (!OutputText.Contains(text, StringComparison.Ordinal))
            {
                await Task.Delay(10, timeout.Token);
            }
        }

        public MonitorCcuEventsCommand Command { get; }

        public ICcuConnectionsStore ConnectionsStore { get; } = A.Fake<ICcuConnectionsStore>();

        public NetworkCredential Credential { get; } = new("Admin", "secret");

        public IHomeMaticXmlRpcApi XmlRpcApi { get; } = A.Fake<IHomeMaticXmlRpcApi>();

        public IHomeMaticJsonRpcClientBuilder JsonRpcClientBuilder { get; } = A.Fake<IHomeMaticJsonRpcClientBuilder>();

        public IHomeMaticJsonRpcClient JsonRpcClient { get; } = A.Fake<IHomeMaticJsonRpcClient>();

        public ICcuXmlRpcEventServerFactory EventServerFactory { get; } = A.Fake<ICcuXmlRpcEventServerFactory>();

        public ICcuXmlRpcEventServer EventServer { get; } = A.Fake<ICcuXmlRpcEventServer>();

        public ICallbackEndpointResolver EndpointResolver { get; } = A.Fake<ICallbackEndpointResolver>();

        public IAnsiConsoleInput Input { get; } = A.Fake<IAnsiConsoleInput>();

        public IConsoleCancelKeySource CancelKeySource { get; } = A.Fake<IConsoleCancelKeySource>();

        public IDisposable CancelKeyRegistration { get; } = A.Fake<IDisposable>();

        public ConcurrentQueue<string> InterfaceIds { get; } = new();

        public ICcuEventHandler? EventHandler { get; private set; }

        public string OutputText => _output.Snapshot();

        public string[] OutputLines => OutputText
            .Split('\n')
            .Select(x => x.TrimEnd('\r'))
            .Where(x => x.Length > 0)
            .ToArray();
    }

    private sealed class LockedTextWriter : TextWriter
    {
        private readonly Lock _lock = new();

        private readonly StringBuilder _text = new();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            lock (_lock)
            {
                _text.Append(value);
            }
        }

        public override void Write(string? value)
        {
            lock (_lock)
            {
                _text.Append(value);
            }
        }

        public string Snapshot()
        {
            lock (_lock)
            {
                return _text.ToString();
            }
        }
    }
}
