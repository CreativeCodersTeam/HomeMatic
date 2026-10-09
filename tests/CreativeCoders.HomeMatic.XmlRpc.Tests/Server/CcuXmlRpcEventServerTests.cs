using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using CreativeCoders.Net.XmlRpc.Server;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Server;

public sealed class CcuXmlRpcEventServerTests : IAsyncLifetime
{
    private const string InterfaceId = "hmc-1234abcd-HomeMaticIp";

    private const string DeviceAddress = "000A1B2C3D4E5F";

    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);

    private readonly List<ICcuXmlRpcEventServer> _servers = [];

    private readonly HttpClient _httpClient = new() { Timeout = OperationTimeout };

    private readonly string _listenUrl = FreeTcpPort.GetLoopbackPrefix();

    private readonly ICcuEventHandler _eventHandler = A.Fake<ICcuEventHandler>();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();

        foreach (var server in _servers)
        {
            await server.DisposeAsync().AsTask().WaitAsync(OperationTimeout);
        }
    }

    [Fact]
    public async Task MultiCall_TwoEventCalls_CallsEventHandlerTwiceInOrderWithInterfaceId()
    {
        // Arrange
        await StartServerAsync();

        var calls = Array(
            MultiCallEntry("event",
                $"<value>{InterfaceId}</value>",
                $"<value>{DeviceAddress}:1</value>",
                "<value>STATE</value>",
                "<value><boolean>1</boolean></value>"),
            MultiCallEntry("event",
                $"<value>{InterfaceId}</value>",
                $"<value>{DeviceAddress}:2</value>",
                "<value>STATE</value>",
                "<value><boolean>0</boolean></value>"));
        var body = MethodCall("system.multicall", $"<param>{calls}</param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:2", "STATE", false))
                .MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task Event_Latin1EncodedStringValueWithUmlaut_ArrivesUnchanged()
    {
        // Arrange
        await StartServerAsync();

        var body = MethodCall("event",
            $"<param><value>{InterfaceId}</value></param>",
            $"<param><value>{DeviceAddress}:1</value></param>",
            "<param><value>NAME</value></param>",
            "<param><value><string>Küche</string></value></param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:1", "NAME", "Küche"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task NewDevices_DeviceDescriptions_CallsHandlerWithInterfaceIdFirst()
    {
        // Arrange
        await StartServerAsync();

        var body = MethodCall("newDevices",
            $"<param><value>{InterfaceId}</value></param>",
            $"<param>{Array(DeviceDescriptionStruct())}</param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.NewDevices(InterfaceId,
                A<DeviceDescription[]>.That.Matches(x => x.Length == 1 && x[0].Address == DeviceAddress)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteDevices_DeviceDescriptions_CallsHandlerWithInterfaceIdFirst()
    {
        // Arrange
        await StartServerAsync();

        var body = MethodCall("deleteDevices",
            $"<param><value>{InterfaceId}</value></param>",
            $"<param>{Array(DeviceDescriptionStruct())}</param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.DeleteDevices(InterfaceId,
                A<DeviceDescription[]>.That.Matches(x => x.Length == 1 && x[0].Address == DeviceAddress)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateDevice_AddressAndHint_CallsHandlerWithInterfaceIdFirst()
    {
        // Arrange
        await StartServerAsync();

        var body = MethodCall("updateDevice",
            $"<param><value>{InterfaceId}</value></param>",
            $"<param><value>{DeviceAddress}</value></param>",
            "<param><value><i4>1</i4></value></param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.UpdateDevice(InterfaceId, DeviceAddress, 1))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListDevices_InterfaceId_ReturnsEmptyArray()
    {
        // Arrange
        await StartServerAsync();

        var body = MethodCall("listDevices", $"<param><value>{InterfaceId}</value></param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var responseXml = XDocument.Load(responseStream);
        var array = responseXml.Descendants("param").Single().Element("value")?.Element("array");
        array.Should().NotBeNull();
        array.Element("data")?.Elements().Should().BeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_StartedServer_ReleasesPortForNewServer()
    {
        // Arrange
        var firstServer = CreateServer();
        await firstServer.StartAsync().WaitAsync(OperationTimeout);
        await firstServer.DisposeAsync().AsTask().WaitAsync(OperationTimeout);

        var sut = CreateServer();

        // Act
        var act = () => sut.StartAsync().WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var sut = CreateServer();
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        var act = async () =>
        {
            await DisposeOnceAsync().WaitAsync(OperationTimeout);
            await DisposeOnceAsync().WaitAsync(OperationTimeout);

            Task DisposeOnceAsync() => sut.DisposeAsync().AsTask();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_ServerNeverStarted_DoesNotThrow()
    {
        // Arrange
        var sut = CreateServer();

        // Act
        var act = () => sut.DisposeAsync().AsTask().WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Event_HandlerThrows_RespondsWithOkAndNoFault()
    {
        // Arrange
        A.CallTo(() => _eventHandler.Event(A<string>._, A<string>._, A<string>._, A<object>._))
            .ThrowsAsync(new InvalidOperationException("Handler failure"));
        await StartServerAsync();

        var body = EventCall($"{DeviceAddress}:1");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
    }

    [Fact]
    public async Task MultiCall_HandlerThrowsForFirstEvent_StillDispatchesSecondEventWithoutFault()
    {
        // Arrange
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:1", A<string>._, A<object>._))
            .ThrowsAsync(new InvalidOperationException("Handler failure"));
        await StartServerAsync();

        var calls = Array(
            MultiCallEntry("event",
                $"<value>{InterfaceId}</value>",
                $"<value>{DeviceAddress}:1</value>",
                "<value>STATE</value>",
                "<value><boolean>1</boolean></value>"),
            MultiCallEntry("event",
                $"<value>{InterfaceId}</value>",
                $"<value>{DeviceAddress}:2</value>",
                "<value>STATE</value>",
                "<value><boolean>0</boolean></value>"));
        var body = MethodCall("system.multicall", $"<param>{calls}</param>");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:2", "STATE", false))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Event_FirstHandlerThrowsAsynchronously_StillCallsSecondHandlerAfterFirst()
    {
        // Arrange
        var secondHandler = A.Fake<ICcuEventHandler>();
        A.CallTo(() => _eventHandler.Event(A<string>._, A<string>._, A<string>._, A<object>._))
            .ThrowsAsync(new InvalidOperationException("Handler failure"));
        await StartServerAsync(NullLoggerFactory.Instance, _eventHandler, secondHandler);

        var body = EventCall($"{DeviceAddress}:1");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => secondHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
                .MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task Event_FirstHandlerThrowsSynchronously_StillCallsSecondHandler()
    {
        // Arrange
        var secondHandler = A.Fake<ICcuEventHandler>();
        A.CallTo(() => _eventHandler.Event(A<string>._, A<string>._, A<string>._, A<object>._))
            .Throws(new InvalidOperationException("Handler failure"));
        await StartServerAsync(NullLoggerFactory.Instance, _eventHandler, secondHandler);

        var body = EventCall($"{DeviceAddress}:1");

        // Act
        using var response = await PostXmlRpcAsync(body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
        A.CallTo(() => secondHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("event")]
    [InlineData("newDevices")]
    [InlineData("deleteDevices")]
    [InlineData("updateDevice")]
    public async Task Callback_HandlerThrows_LogsOneErrorWithExceptionHandlerTypeAndMethodName(string methodName)
    {
        // Arrange
        var handlerException = new InvalidOperationException("Handler failure");
        ThrowFromAllCallbacks(_eventHandler, handlerException);
        var (loggerFactory, logEntries) = CreateCapturingLoggerFactory();
        await StartServerAsync(loggerFactory, _eventHandler);

        // Act
        using var response = await PostXmlRpcAsync(CallbackCall(methodName));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var error = logEntries.Should().ContainSingle(x => x.Level == LogLevel.Error).Subject;
        error.Exception.Should().BeSameAs(handlerException);
        error.Message.Should().Contain($"CCU callback {methodName}")
            .And.Contain(_eventHandler.GetType().FullName);
    }

    [Theory]
    [InlineData("newDevices")]
    [InlineData("deleteDevices")]
    [InlineData("updateDevice")]
    public async Task DeviceCallback_HandlerThrows_RespondsWithOkAndStillCallsSecondHandler(string methodName)
    {
        // Arrange
        var secondHandler = A.Fake<ICcuEventHandler>();
        ThrowFromAllCallbacks(_eventHandler, new InvalidOperationException("Handler failure"));
        await StartServerAsync(NullLoggerFactory.Instance, _eventHandler, secondHandler);

        // Act
        using var response = await PostXmlRpcAsync(CallbackCall(methodName));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
        A.CallTo(secondHandler)
            .Where(call => call.Method.Name == ToHandlerMethodName(methodName)
                           && call.GetArgument<string>(0) == InterfaceId)
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Event_AllHandlersThrow_CallsEachHandlerAndLogsOneErrorPerHandler()
    {
        // Arrange
        var secondHandler = A.Fake<ICcuEventHandler>();
        var firstException = new InvalidOperationException("First handler failure");
        var secondException = new InvalidOperationException("Second handler failure");
        ThrowFromAllCallbacks(_eventHandler, firstException);
        ThrowFromAllCallbacks(secondHandler, secondException);
        var (loggerFactory, logEntries) = CreateCapturingLoggerFactory();
        await StartServerAsync(loggerFactory, _eventHandler, secondHandler);

        // Act
        using var response = await PostXmlRpcAsync(EventCall($"{DeviceAddress}:1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
        A.CallTo(() => secondHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly();
        logEntries.Where(x => x.Level == LogLevel.Error).Select(x => x.Exception)
            .Should().Equal(firstException, secondException);
    }

    [Fact]
    public async Task Event_HandlerTaskCanceled_RespondsWithOkAndStillCallsSecondHandler()
    {
        // Arrange
        var secondHandler = A.Fake<ICcuEventHandler>();
        A.CallTo(() => _eventHandler.Event(A<string>._, A<string>._, A<string>._, A<object>._))
            .Returns(Task.FromCanceled(new CancellationToken(true)));
        await StartServerAsync(NullLoggerFactory.Instance, _eventHandler, secondHandler);

        // Act
        using var response = await PostXmlRpcAsync(EventCall($"{DeviceAddress}:1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadFaultsAsync(response)).Should().BeEmpty();
        A.CallTo(() => secondHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Event_HandlerSucceeds_LogsNoError()
    {
        // Arrange
        var (loggerFactory, logEntries) = CreateCapturingLoggerFactory();
        await StartServerAsync(loggerFactory, _eventHandler);

        // Act
        using var response = await PostXmlRpcAsync(EventCall($"{DeviceAddress}:1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        A.CallTo(() => _eventHandler.Event(InterfaceId, $"{DeviceAddress}:1", "STATE", true))
            .MustHaveHappenedOnceExactly();
        logEntries.Should().NotContain(x => x.Level == LogLevel.Error);
    }

    [Fact]
    public void RegisterEventHandler_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateServer();

        // Act
        var act = () => sut.RegisterEventHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task DisposeAsync_StartFailedBecausePortInUse_DoesNotThrow()
    {
        // Arrange
        var blockingServer = CreateServer();
        await blockingServer.StartAsync().WaitAsync(OperationTimeout);

        var sut = CreateServer();
        var startAct = () => sut.StartAsync().WaitAsync(OperationTimeout);
        await startAct.Should().ThrowAsync<CcuEventServerStartException>();

        // Act
        var act = () => sut.DisposeAsync().AsTask().WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_StopThrows_StillDisposesXmlRpcServerAndRethrows()
    {
        // Arrange
        var xmlRpcServer = A.Fake<IXmlRpcServer>();
        A.CallTo(() => xmlRpcServer.Urls).Returns(new List<string>());
        A.CallTo(() => xmlRpcServer.StopAsync()).ThrowsAsync(new InvalidOperationException("Stop failure"));
        var sut = new CcuXmlRpcEventServer(xmlRpcServer, NullLogger<CcuXmlRpcEventServer>.Instance)
        {
            ServerUrl = _listenUrl
        };
        await sut.StartAsync();

        // Act
        var act = () => sut.DisposeAsync().AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        A.CallTo(() => xmlRpcServer.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DisposeAsync_AfterExplicitStop_DoesNotStopAgainButDisposesOnce()
    {
        // Arrange
        var xmlRpcServer = A.Fake<IXmlRpcServer>();
        A.CallTo(() => xmlRpcServer.Urls).Returns(new List<string>());
        var sut = new CcuXmlRpcEventServer(xmlRpcServer, NullLogger<CcuXmlRpcEventServer>.Instance)
        {
            ServerUrl = _listenUrl
        };
        await sut.StartAsync();
        await sut.StopAsync();

        // Act
        await sut.DisposeAsync();

        // Assert
        A.CallTo(() => xmlRpcServer.StopAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => xmlRpcServer.Dispose()).MustHaveHappenedOnceExactly();
    }

    private ICcuXmlRpcEventServer CreateServer(ILoggerFactory? loggerFactory = null)
    {
        var server = new CcuXmlRpcEventServerFactory(loggerFactory ?? NullLoggerFactory.Instance).Create(_listenUrl);

        _servers.Add(server);

        return server;
    }

    private Task StartServerAsync()
    {
        return StartServerAsync(NullLoggerFactory.Instance, _eventHandler);
    }

    private async Task StartServerAsync(ILoggerFactory loggerFactory, params ICcuEventHandler[] eventHandlers)
    {
        var server = CreateServer(loggerFactory);

        foreach (var eventHandler in eventHandlers)
        {
            server.RegisterEventHandler(eventHandler);
        }

        await server.StartAsync().WaitAsync(OperationTimeout);
    }

    private static async Task<IReadOnlyList<XElement>> ReadFaultsAsync(HttpResponseMessage response)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var responseXml = XDocument.Load(responseStream);

        return responseXml.Descendants("fault").ToArray();
    }

    private static string EventCall(string address)
    {
        return MethodCall("event",
            $"<param><value>{InterfaceId}</value></param>",
            $"<param><value>{address}</value></param>",
            "<param><value>STATE</value></param>",
            "<param><value><boolean>1</boolean></value></param>");
    }

    private static string CallbackCall(string methodName)
    {
        return methodName switch
        {
            "event" => EventCall($"{DeviceAddress}:1"),
            "updateDevice" => MethodCall(methodName,
                $"<param><value>{InterfaceId}</value></param>",
                $"<param><value>{DeviceAddress}</value></param>",
                "<param><value><i4>1</i4></value></param>"),
            _ => MethodCall(methodName,
                $"<param><value>{InterfaceId}</value></param>",
                $"<param>{Array(DeviceDescriptionStruct())}</param>")
        };
    }

    private static void ThrowFromAllCallbacks(ICcuEventHandler eventHandler, Exception exception)
    {
        A.CallTo(() => eventHandler.Event(A<string>._, A<string>._, A<string>._, A<object>._))
            .ThrowsAsync(exception);
        A.CallTo(() => eventHandler.NewDevices(A<string>._, A<DeviceDescription[]>._))
            .ThrowsAsync(exception);
        A.CallTo(() => eventHandler.DeleteDevices(A<string>._, A<DeviceDescription[]>._))
            .ThrowsAsync(exception);
        A.CallTo(() => eventHandler.UpdateDevice(A<string>._, A<string>._, A<int>._))
            .ThrowsAsync(exception);
    }

    private static (ILoggerFactory LoggerFactory, ConcurrentQueue<LogEntry> Entries) CreateCapturingLoggerFactory()
    {
        var entries = new ConcurrentQueue<LogEntry>();
        var logger = A.Fake<ILogger>();
        A.CallTo(logger)
            .Where(call => call.Method.Name == nameof(ILogger.Log))
            .Invokes(call => entries.Enqueue(new LogEntry(
                call.GetArgument<LogLevel>(0),
                call.GetArgument<Exception?>(3),
                Convert.ToString(call.Arguments[2], CultureInfo.InvariantCulture) ?? string.Empty)));
        var loggerFactory = A.Fake<ILoggerFactory>();
        A.CallTo(() => loggerFactory.CreateLogger(A<string>._)).Returns(logger);

        return (loggerFactory, entries);
    }

    private static string ToHandlerMethodName(string methodName)
    {
        return methodName switch
        {
            "newDevices" => nameof(ICcuEventHandler.NewDevices),
            "deleteDevices" => nameof(ICcuEventHandler.DeleteDevices),
            _ => nameof(ICcuEventHandler.UpdateDevice)
        };
    }

    private Task<HttpResponseMessage> PostXmlRpcAsync(string xml)
    {
        var content = new ByteArrayContent(Encoding.Latin1.GetBytes(xml));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

        return _httpClient.PostAsync(_listenUrl, content);
    }

    private static string MethodCall(string methodName, params string[] parameters)
    {
        return "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" +
               $"<methodCall><methodName>{methodName}</methodName>" +
               $"<params>{string.Concat(parameters)}</params>" +
               "</methodCall>";
    }

    private static string MultiCallEntry(string methodName, params string[] parameterValues)
    {
        return "<value><struct>" +
               $"<member><name>methodName</name><value>{methodName}</value></member>" +
               $"<member><name>params</name>{Array(parameterValues)}</member>" +
               "</struct></value>";
    }

    private static string Array(params string[] values)
    {
        return $"<value><array><data>{string.Concat(values)}</data></array></value>";
    }

    private static string DeviceDescriptionStruct()
    {
        return "<value><struct>" +
               $"<member><name>ADDRESS</name><value>{DeviceAddress}</value></member>" +
               "<member><name>TYPE</name><value>HmIP-BROLL</value></member>" +
               "<member><name>PARENT</name><value></value></member>" +
               "<member><name>VERSION</name><value><i4>1</i4></value></member>" +
               "</struct></value>";
    }

    private sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);
}
