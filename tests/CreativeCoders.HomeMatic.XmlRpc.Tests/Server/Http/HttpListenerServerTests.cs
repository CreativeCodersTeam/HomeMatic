using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using CreativeCoders.HomeMatic.XmlRpc.Server.Http;
using CreativeCoders.Net.Servers.Http;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Server.Http;

public sealed class HttpListenerServerTests : IAsyncLifetime
{
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);

    private readonly List<HttpListenerServer> _servers = [];

    private readonly HttpClient _httpClient = new() { Timeout = OperationTimeout };

    private readonly string _prefix = FreeTcpPort.GetLoopbackPrefix();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();

        foreach (var server in _servers)
        {
            try
            {
                await server.StopAsync().WaitAsync(OperationTimeout);
            }
            finally
            {
                server.Dispose();
            }
        }
    }

    [Fact]
    public async Task StartAsync_PostRequest_ReachesHandlerAndReturnsWrittenBodyWithStatus200()
    {
        // Arrange
        var handler = A.Fake<IHttpRequestHandler>();
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .ReturnsLazily(async (IHttpRequest request, IHttpResponse response) =>
            {
                var requestBody = await request.Body.ReadAsStringAsync();
                await response.Body.WriteAsync($"echo:{requestBody}");
            });

        var sut = CreateServer(handler, _prefix);

        // Act
        await sut.StartAsync().WaitAsync(OperationTimeout);
        using var response = await PostAsync(_prefix, "ping");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("echo:ping");
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsync_PortAlreadyInUseByOtherServer_ThrowsStartExceptionWrappingListenerError()
    {
        // Arrange
        var firstServer = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await firstServer.StartAsync().WaitAsync(OperationTimeout);

        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);

        // Act
        var act = () => sut.StartAsync().WaitAsync(OperationTimeout);

        // Assert
        // The reason depends on the platform: the managed listener on Linux and macOS reports a conflict with a
        // listener of the same process as error 400, Windows as ERROR_ALREADY_EXISTS.
        var exception = (await act.Should().ThrowAsync<CcuEventServerStartException>()).Which;
        exception.InnerException.Should().BeOfType<HttpListenerException>()
            .Which.Message.Should().Be(exception.Message);
    }

    [Theory]
    [InlineData(183, true, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(32, true, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(10048, true, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(5, true, CcuEventServerStartFailure.AccessDenied)]
    [InlineData(10013, true, CcuEventServerStartFailure.AccessDenied)]
    [InlineData(98, true, CcuEventServerStartFailure.Other)]
    [InlineData(13, true, CcuEventServerStartFailure.Other)]
    [InlineData(400, true, CcuEventServerStartFailure.Other)]
    [InlineData(98, false, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(48, false, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(10048, false, CcuEventServerStartFailure.AddressInUse)]
    [InlineData(13, false, CcuEventServerStartFailure.AccessDenied)]
    [InlineData(10013, false, CcuEventServerStartFailure.AccessDenied)]
    [InlineData(5, false, CcuEventServerStartFailure.Other)]
    [InlineData(32, false, CcuEventServerStartFailure.Other)]
    [InlineData(183, false, CcuEventServerStartFailure.Other)]
    [InlineData(400, false, CcuEventServerStartFailure.Other)]
    [InlineData(0, false, CcuEventServerStartFailure.Other)]
    public void ToStartException_ErrorCodeOnPlatform_MapsToReasonAndKeepsMessageAndInnerException(int errorCode,
        bool isWindows, CcuEventServerStartFailure expectedReason)
    {
        // Arrange
        var listenerException = new HttpListenerException(errorCode, "Prefix [conflict]");

        // Act
        var exception = HttpListenerServer.ToStartException(listenerException, isWindows);

        // Assert
        exception.Reason.Should().Be(expectedReason);
        exception.Message.Should().Be("Prefix [conflict]");
        exception.InnerException.Should().BeSameAs(listenerException);
    }

    [Fact]
    public async Task HandleRequest_HandlerThrows_Returns500AndNextRequestStillWorks()
    {
        // Arrange
        var callCount = 0;
        var handler = A.Fake<IHttpRequestHandler>();
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .ReturnsLazily(async (IHttpRequest _, IHttpResponse response) =>
            {
                if (Interlocked.Increment(ref callCount) == 1)
                {
                    throw new InvalidOperationException("Handler failure");
                }

                await response.Body.WriteAsync("ok");
            });

        var sut = CreateServer(handler, _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        using var failedResponse = await PostAsync(_prefix, "first");
        using var nextResponse = await PostAsync(_prefix, "second");

        // Assert
        failedResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        nextResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await nextResponse.Content.ReadAsStringAsync()).Should().Be("ok");
    }

    [Fact]
    public async Task StopAsync_CalledTwiceThenDispose_DoesNotThrow()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        var act = async () =>
        {
            await sut.StopAsync().WaitAsync(OperationTimeout);
            await sut.StopAsync().WaitAsync(OperationTimeout);
            sut.Dispose();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleRequest_TwoRequestsSentOneAfterAnother_ReachHandlerInSendOrder()
    {
        // Arrange
        var receivedBodies = new ConcurrentQueue<string>();
        var handler = A.Fake<IHttpRequestHandler>();
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .ReturnsLazily(async (IHttpRequest request, IHttpResponse _) =>
            {
                receivedBodies.Enqueue(await request.Body.ReadAsStringAsync());
            });

        var sut = CreateServer(handler, _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        using var firstResponse = await PostAsync(_prefix, "first");
        using var secondResponse = await PostAsync(_prefix, "second");

        // Assert
        receivedBodies.Should().Equal("first", "second");
    }

    [Fact]
    public async Task HandleRequest_RequestsSentConcurrently_AreProcessedOneAtATime()
    {
        // Arrange
        var activeCalls = 0;
        var maxActiveCalls = 0;
        var handler = A.Fake<IHttpRequestHandler>();
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .ReturnsLazily(async (IHttpRequest _, IHttpResponse _) =>
            {
                var active = Interlocked.Increment(ref activeCalls);
                InterlockedMax(ref maxActiveCalls, active);
                await Task.Delay(50);
                Interlocked.Decrement(ref activeCalls);
            });

        var sut = CreateServer(handler, _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(i => PostAsync(_prefix, $"request {i}")));

        // Assert
        responses.Should().AllSatisfy(x => x.StatusCode.Should().Be(HttpStatusCode.OK));
        maxActiveCalls.Should().Be(1);
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .MustHaveHappened(4, Times.Exactly);

        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task StartAsync_AlreadyStarted_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        // Act
        var act = () => sut.StartAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        sut.Dispose();

        // Act
        var act = () => sut.StartAsync();

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task StopAsync_ServerNeverStarted_DoesNotThrow()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);

        // Act
        var act = () => sut.StopAsync().WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_StartedServer_ReleasesPrefixForNewServer()
    {
        // Arrange
        var firstServer = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await firstServer.StartAsync().WaitAsync(OperationTimeout);
        await firstServer.StopAsync().WaitAsync(OperationTimeout);

        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);

        // Act
        var act = () => sut.StartAsync().WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Dispose_StartedServerWithoutStop_ReleasesPrefixAndLaterStopDoesNotThrow()
    {
        // Arrange
        var firstServer = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await firstServer.StartAsync().WaitAsync(OperationTimeout);

        // Act
        firstServer.Dispose();
        var stopAct = () => firstServer.StopAsync().WaitAsync(OperationTimeout);
        var startNewServerAct = () =>
            CreateServer(A.Fake<IHttpRequestHandler>(), _prefix).StartAsync().WaitAsync(OperationTimeout);

        // Assert
        await stopAct.Should().NotThrowAsync();
        await startNewServerAct.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);
        sut.Dispose();

        // Act
        var act = () => sut.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task HandleRequest_ClientSendsHeadersButNoBody_RequestTimesOutAndNextRequestIsProcessed()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMilliseconds(500);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        using var response = await PostAsync(_prefix, "after stall");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("echo:after stall");
    }

    [Fact]
    public async Task StopAsync_StalledRequestInFlight_CompletesPromptly()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMinutes(5);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        var act = () => sut.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_StalledRequestInFlight_ClosesStalledConnectionWithoutSuccessResponse()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMinutes(5);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        await sut.StopAsync().WaitAsync(OperationTimeout);
        var receivedText = await ReadUntilConnectionClosedAsync(stalledClient).WaitAsync(OperationTimeout);

        // Assert
        receivedText.Should().NotStartWith("HTTP/1.1 2");
    }

    [Fact]
    public async Task StopAsync_CalledConcurrentlyWithStalledRequestInFlight_AllCallsComplete()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMinutes(5);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        var act = () => Task.WhenAll(sut.StopAsync(), sut.StopAsync()).WaitAsync(OperationTimeout);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_AfterStop_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateServer(A.Fake<IHttpRequestHandler>(), _prefix);
        await sut.StartAsync().WaitAsync(OperationTimeout);
        await sut.StopAsync().WaitAsync(OperationTimeout);

        // Act
        var act = () => sut.StartAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Dispose_StalledRequestInFlightWithoutStop_DoesNotThrowAndClosesStalledConnection()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMinutes(5);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        var act = () => sut.Dispose();

        // Assert
        act.Should().NotThrow();
        var readUntilClosed = () => ReadUntilConnectionClosedAsync(stalledClient);
        await readUntilClosed.Should().CompleteWithinAsync(OperationTimeout);
    }

    [Fact]
    public async Task TryAbort_CalledTwice_DoesNotThrow()
    {
        // Arrange
        using var listener = new HttpListener();
        listener.Prefixes.Add(_prefix);
        listener.Start();

        var contextTask = listener.GetContextAsync();
        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        var context = await contextTask.WaitAsync(OperationTimeout);

        HttpListenerServer.TryAbort(context.Response);

        // Act
        var act = () => HttpListenerServer.TryAbort(context.Response);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task TryAbort_ListenerClosedWhileRequestInFlight_DoesNotThrow()
    {
        // Arrange
        using var listener = new HttpListener();
        listener.Prefixes.Add(_prefix);
        listener.Start();

        var contextTask = listener.GetContextAsync();
        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        var context = await contextTask.WaitAsync(OperationTimeout);

        listener.Close();

        // Act
        var act = () => HttpListenerServer.TryAbort(context.Response);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task TryAbort_ListenerOpen_ClosesConnection()
    {
        // Arrange
        using var listener = new HttpListener();
        listener.Prefixes.Add(_prefix);
        listener.Start();

        var contextTask = listener.GetContextAsync();
        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        var context = await contextTask.WaitAsync(OperationTimeout);

        // Act
        HttpListenerServer.TryAbort(context.Response);

        // Assert
        var readUntilClosed = () => ReadUntilConnectionClosedAsync(stalledClient);
        await readUntilClosed.Should().CompleteWithinAsync(OperationTimeout);
    }

    [Fact]
    public void RequestTimeout_NotSet_IsThirtySeconds()
    {
        // Arrange
        using var sut = new HttpListenerServer();

        // Act
        var requestTimeout = sut.RequestTimeout;

        // Assert
        requestTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RequestTimeout_SetToZeroOrNegative_ThrowsArgumentOutOfRangeException(int milliseconds)
    {
        // Arrange
        using var sut = new HttpListenerServer();

        // Act
        var act = () => sut.RequestTimeout = TimeSpan.FromMilliseconds(milliseconds);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RequestTimeout_SetAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var sut = new HttpListenerServer();

        // Act
        var act = () => sut.RequestTimeout = TimeSpan.FromMilliseconds(int.MaxValue) + TimeSpan.FromMilliseconds(1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RequestTimeout_SetToMaximum_IsAccepted()
    {
        // Arrange
        using var sut = new HttpListenerServer();

        // Act
        sut.RequestTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

        // Assert
        sut.RequestTimeout.Should().Be(TimeSpan.FromMilliseconds(int.MaxValue));
    }

    [Fact]
    public async Task HandleRequest_ClientSendsHeadersButNoBody_ClosesStalledConnectionWithoutSuccessResponse()
    {
        // Arrange
        var stalledRequestReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = CreateEchoHandler(stalledRequestReceived);

        var sut = CreateServer(handler, _prefix);
        sut.RequestTimeout = TimeSpan.FromMilliseconds(500);
        await sut.StartAsync().WaitAsync(OperationTimeout);

        using var stalledClient = await SendHeadersWithoutBodyAsync(_prefix);
        await stalledRequestReceived.Task.WaitAsync(OperationTimeout);

        // Act
        var receivedText = await ReadUntilConnectionClosedAsync(stalledClient).WaitAsync(OperationTimeout);

        // Assert
        // http.sys resets the connection without a response; the managed HttpListener sends the status first.
        receivedText.Should().Match(x => x.Length == 0 || x.StartsWith("HTTP/1.1 408 "));
    }

    private static async Task<string> ReadUntilConnectionClosedAsync(TcpClient tcpClient)
    {
        var buffer = new byte[1024];
        var receivedText = new StringBuilder();

        try
        {
            int read;
            while ((read = await tcpClient.GetStream().ReadAsync(buffer)) > 0)
            {
                receivedText.Append(Encoding.ASCII.GetString(buffer, 0, read));
            }
        }
        catch (IOException)
        {
            // A reset connection counts as closed.
        }

        return receivedText.ToString();
    }

    private static IHttpRequestHandler CreateEchoHandler(TaskCompletionSource firstRequestReceived)
    {
        var handler = A.Fake<IHttpRequestHandler>();
        A.CallTo(() => handler.ProcessAsync(A<IHttpRequest>._, A<IHttpResponse>._))
            .ReturnsLazily(async (IHttpRequest request, IHttpResponse response) =>
            {
                firstRequestReceived.TrySetResult();
                var requestBody = await request.Body.ReadAsStringAsync();
                await response.Body.WriteAsync($"echo:{requestBody}");
            });

        return handler;
    }

    private static async Task<TcpClient> SendHeadersWithoutBodyAsync(string prefix)
    {
        var uri = new Uri(prefix);
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(uri.Host, uri.Port);

        var header = $"POST {uri.AbsolutePath} HTTP/1.1\r\nHost: {uri.Authority}\r\n" +
                     "Content-Type: text/plain\r\nContent-Length: 100\r\n\r\n";
        await tcpClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes(header));

        return tcpClient;
    }

    private static void InterlockedMax(ref int target, int value)
    {
        var current = Volatile.Read(ref target);
        while (value > current)
        {
            var previous = Interlocked.CompareExchange(ref target, value, current);
            if (previous == current)
            {
                return;
            }

            current = previous;
        }
    }

    private HttpListenerServer CreateServer(IHttpRequestHandler handler, string prefix)
    {
        var server = new HttpListenerServer();
        server.RegisterRequestHandler(handler);
        server.Urls.Add(prefix);

        _servers.Add(server);

        return server;
    }

    private Task<HttpResponseMessage> PostAsync(string url, string body)
    {
        return _httpClient.PostAsync(url, new StringContent(body, Encoding.UTF8, "text/plain"));
    }
}
