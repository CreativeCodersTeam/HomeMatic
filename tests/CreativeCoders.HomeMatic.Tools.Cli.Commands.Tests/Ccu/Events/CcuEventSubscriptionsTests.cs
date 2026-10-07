using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;
using AwesomeAssertions;
using CreativeCoders.HomeMatic.Tools.Cli.Commands.Ccu.Events;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;

namespace CreativeCoders.HomeMatic.Tools.Cli.Commands.Tests.Ccu.Events;

public class CcuEventSubscriptionsTests
{
    private const string CallbackUrl = "http://192.168.1.20:53817/";

    private static readonly Uri CcuUrl = new("http://ccu.example.local/");

    private static readonly TimeSpan SubscribeTimeout = TimeSpan.FromMilliseconds(100);

    private static readonly TimeSpan UnsubscribeTimeout = TimeSpan.FromMilliseconds(100);

    private static readonly CcuDeviceKind[] SubscribedKinds =
        [CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp, CcuDeviceKind.HomeMaticWired];

    [Fact]
    public async Task SubscribeAsync_AllInterfacesAvailable_CallsInitOncePerKindWithSessionInterfaceId()
    {
        // Arrange
        var context = new ApiContext();
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Subscribed.Should().Equal(SubscribedKinds);
        result.Failed.Should().BeEmpty();
        sut.SessionId.Should().MatchRegex("^[0-9a-f]{8}$");

        foreach (var kind in SubscribedKinds)
        {
            var api = context.Apis[kind];
            A.CallTo(() => api.InitAsync(CallbackUrl, $"hmc-{sut.SessionId}-{kind}"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => api.InitAsync(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public void Ctor_ApiAddresses_UseCcuUrlWithInterfacePort()
    {
        // Arrange
        var context = new ApiContext();

        // Act
        context.CreateSut();

        // Assert
        context.RequestedUrls.Should().BeEquivalentTo(
        [
            new Uri("http://ccu.example.local:2001/"),
            new Uri("http://ccu.example.local:2010/"),
            new Uri("http://ccu.example.local:2000/")
        ]);
    }

    [Fact]
    public async Task SubscribeAsync_OneKindThrows_ReturnsTwoSubscribedAndOneFailedWithErrorMessage()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException("Connection refused"));
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Subscribed.Should().Equal(CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp);
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticWired, "connection failed on port 2000 (Connection refused)"));
    }

    [Fact]
    public async Task SubscribeAsync_OneKindThrowsSynchronously_ReturnsItAsFailed()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMatic].InitAsync(A<string>._, A<string>._))
            .Throws(new InvalidOperationException("sync failure"));
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Subscribed.Should().Equal(CcuDeviceKind.HomeMaticIp, CcuDeviceKind.HomeMaticWired);
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMatic, "sync failure"));
    }

    [Theory]
    [InlineData(CcuDeviceKind.HomeMatic, 2001)]
    [InlineData(CcuDeviceKind.HomeMaticIp, 2010)]
    [InlineData(CcuDeviceKind.HomeMaticWired, 2000)]
    public async Task SubscribeAsync_InitThrowsXmlException_FailsWithNoXmlRpcServiceOnInterfacePort(
        CcuDeviceKind kind,
        int port)
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[kind].InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new XmlException("For security reasons DTD is prohibited in this XML document."));
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((kind, $"no XML-RPC service on port {port}"));
    }

    [Fact]
    public async Task SubscribeAsync_InitThrowsHttpRequestExceptionWithSocketException_FailsWithSocketReason()
    {
        // Arrange
        var context = new ApiContext();
        var socketException = new SocketException((int)SocketError.ConnectionRefused);
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(new HttpRequestException(
                $"{socketException.Message} (ccu.example.local:2000)", socketException));
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticWired,
                $"connection failed on port 2000 ({socketException.Message})"));
    }

    [Fact]
    public async Task SubscribeAsync_InitThrowsSocketException_FailsWithConnectionFailedOnInterfacePort()
    {
        // Arrange
        var context = new ApiContext();
        var socketException = new SocketException((int)SocketError.HostUnreachable);
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticIp].InitAsync(A<string>._, A<string>._))
            .ThrowsAsync(socketException);
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticIp,
                $"connection failed on port 2010 ({socketException.Message})"));
    }

    [Fact]
    public async Task SubscribeAsync_AllKindsThrow_SubscribedIsEmpty()
    {
        // Arrange
        var context = new ApiContext();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, A<string>._))
                .ThrowsAsync(new HttpRequestException("Connection refused"));
        }

        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl);

        // Assert
        result.Subscribed.Should().BeEmpty();
        result.Failed.Select(x => x.Kind).Should().Equal(SubscribedKinds);
    }

    [Fact]
    public async Task SubscribeAsync_InitNeverCompletes_FailsWithTimeoutAfterSubscribeTimeout()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticIp].InitAsync(A<string>._, A<string>._))
            .Returns(neverCompletes.Task);
        var sut = context.CreateSut();

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        result.Subscribed.Should().Equal(CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticWired);
        result.Failed.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticIp, "timeout"));
    }

    [Fact]
    public async Task SubscribeAsync_InitsRunInParallel_AllStartBeforeAnyCompletes()
    {
        // Arrange
        var context = new ApiContext();
        var release = new TaskCompletionSource();
        var startedCount = 0;
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, A<string>._))
                .ReturnsLazily(() =>
                {
                    if (Interlocked.Increment(ref startedCount) == SubscribedKinds.Length)
                    {
                        release.SetResult();
                    }

                    return release.Task;
                });
        }

        var sut = new CcuEventSubscriptions(context.Builder, CcuUrl, TimeSpan.FromSeconds(5), UnsubscribeTimeout);

        // Act
        var result = await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        result.Subscribed.Should().Equal(SubscribedKinds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SubscribeAsync_BlankCallbackUrl_ThrowsArgumentExceptionWithoutInitCalls(string callbackUrl)
    {
        // Arrange
        var context = new ApiContext();
        var sut = context.CreateSut();

        // Act
        var act = () => sut.SubscribeAsync(callbackUrl);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        }
    }

    [Fact]
    public void TryGetKind_CaseVariantOfIssuedInterfaceId_ReturnsFalse()
    {
        // Arrange
        var sut = new ApiContext().CreateSut();

        // Act
        var found = sut.TryGetKind($"HMC-{sut.SessionId}-{CcuDeviceKind.HomeMaticIp}".ToUpperInvariant(), out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void TryGetKind_IssuedInterfaceId_ReturnsTrueAndKind()
    {
        // Arrange
        var sut = new ApiContext().CreateSut();

        foreach (var expectedKind in SubscribedKinds)
        {
            // Act
            var found = sut.TryGetKind($"hmc-{sut.SessionId}-{expectedKind}", out var kind);

            // Assert
            found.Should().BeTrue();
            kind.Should().Be(expectedKind);
        }
    }

    [Theory]
    [InlineData("hmc-00000000-HomeMaticIp")]
    [InlineData("")]
    [InlineData("other-client")]
    public void TryGetKind_ForeignInterfaceId_ReturnsFalse(string interfaceId)
    {
        // Arrange
        var sut = new ApiContext().CreateSut();

        // Act
        var found = sut.TryGetKind(interfaceId, out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void SessionId_TwoInstances_AreDifferent()
    {
        // Arrange
        var context = new ApiContext();

        // Act
        var first = context.CreateSut();
        var second = context.CreateSut();

        // Assert
        first.SessionId.Should().NotBe(second.SessionId);
        Regex.IsMatch(first.SessionId, "^[0-9a-f]{8}$").Should().BeTrue();
    }

    [Fact]
    public async Task UnsubscribeAsync_AllSubscribed_CallsInitWithEmptyInterfaceIdOncePerKind()
    {
        // Arrange
        var context = new ApiContext();
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().BeEmpty();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(CallbackUrl, string.Empty)).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_OneKindNotSubscribed_SkipsThatKind()
    {
        // Arrange
        var context = new ApiContext();
        var wiredApi = context.Apis[CcuDeviceKind.HomeMaticWired];
        A.CallTo(() => wiredApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .ThrowsAsync(new HttpRequestException("Connection refused"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().BeEmpty();
        A.CallTo(() => wiredApi.InitAsync(A<string>._, string.Empty)).MustNotHaveHappened();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMatic].InitAsync(CallbackUrl, string.Empty))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticIp].InitAsync(CallbackUrl, string.Empty))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsubscribeAsync_NotSubscribedYet_DoesNothing()
    {
        // Arrange
        var context = new ApiContext();
        var sut = context.CreateSut();

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().BeEmpty();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_InitsRunInParallel_AllStartBeforeAnyCompletes()
    {
        // Arrange
        var context = new ApiContext();
        var release = new TaskCompletionSource();
        var startedCount = 0;
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, string.Empty))
                .ReturnsLazily(() =>
                {
                    if (Interlocked.Increment(ref startedCount) == SubscribedKinds.Length)
                    {
                        release.SetResult();
                    }

                    return release.Task;
                });
        }

        var sut = new CcuEventSubscriptions(context.Builder, CcuUrl, SubscribeTimeout, TimeSpan.FromSeconds(5));
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        failures.Should().BeEmpty();
        startedCount.Should().Be(SubscribedKinds.Length);
    }

    [Fact]
    public async Task UnsubscribeAsync_OneKindThrows_ReturnsFailureAndUnsubscribesOthers()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticIp].InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Connection reset"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticIp, "connection failed on port 2010 (Connection reset)"));
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMatic].InitAsync(CallbackUrl, string.Empty))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(CallbackUrl, string.Empty))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsubscribeAsync_OneKindThrowsSynchronously_ReturnsFailure()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMatic].InitAsync(A<string>._, string.Empty))
            .Throws(new InvalidOperationException("sync failure"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().ContainSingle().Which.Should().Be((CcuDeviceKind.HomeMatic, "sync failure"));
    }

    [Fact]
    public async Task UnsubscribeAsync_InitThrowsXmlException_ReturnsNoXmlRpcServiceOnInterfacePort()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMatic].InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new XmlException("Data at the root level is invalid."));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMatic, "no XML-RPC service on port 2001"));
    }

    [Fact]
    public async Task UnsubscribeAsync_InitThrowsNestedSocketException_ReturnsInnermostSocketReason()
    {
        // Arrange
        var context = new ApiContext();
        var socketException = new SocketException((int)SocketError.ConnectionReset);
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Error while copying content to a stream.",
                new IOException("Unable to read data from the transport connection.", socketException)));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticWired,
                $"connection failed on port 2000 ({socketException.Message})"));
    }

    [Fact]
    public async Task UnsubscribeAsync_InitNeverCompletes_FailsWithTimeoutAfterUnsubscribeTimeout()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(A<string>._, string.Empty))
            .Returns(neverCompletes.Task);
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        failures.Should().ContainSingle().Which.Should().Be((CcuDeviceKind.HomeMaticWired, "timeout"));
    }

    [Fact]
    public async Task UnsubscribeAsync_CalledTwice_SecondCallDoesNothing()
    {
        // Arrange
        var context = new ApiContext();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticIp].InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Connection reset"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl);
        await sut.UnsubscribeAsync();

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().BeEmpty();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, string.Empty)).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_SubscribeTimedOut_UnsubscribesTimedOutKindToo()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        var ipApi = context.Apis[CcuDeviceKind.HomeMaticIp];
        A.CallTo(() => ipApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .Returns(neverCompletes.Task);
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        failures.Should().BeEmpty();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(CallbackUrl, string.Empty)).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_SubscribeTimedOutAndUnsubscribeThrows_ReturnsFailure()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        var ipApi = context.Apis[CcuDeviceKind.HomeMaticIp];
        A.CallTo(() => ipApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .Returns(neverCompletes.Task);
        A.CallTo(() => ipApi.InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Connection reset"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        failures.Should().ContainSingle()
            .Which.Should().Be((CcuDeviceKind.HomeMaticIp, "connection failed on port 2010 (Connection reset)"));
    }

    [Fact]
    public async Task UnsubscribeAsync_SubscribeAndUnsubscribeTimeOut_ReturnsTimeoutFailure()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        A.CallTo(() => context.Apis[CcuDeviceKind.HomeMaticWired].InitAsync(A<string>._, A<string>._))
            .Returns(neverCompletes.Task);
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        failures.Should().ContainSingle().Which.Should().Be((CcuDeviceKind.HomeMaticWired, "timeout"));
    }

    [Fact]
    public async Task UnsubscribeAsync_AllSubscribesTimedOut_UnsubscribesEveryKind()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
                .Returns(neverCompletes.Task);
        }

        var sut = context.CreateSut();
        var result = await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        var failures = await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        result.Subscribed.Should().BeEmpty();
        failures.Should().BeEmpty();
        foreach (var api in context.Apis.Values)
        {
            A.CallTo(() => api.InitAsync(CallbackUrl, string.Empty)).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_SubscribeTimedOutAndCalledTwice_SecondCallDoesNothing()
    {
        // Arrange
        var context = new ApiContext();
        var neverCompletes = new TaskCompletionSource();
        var ipApi = context.Apis[CcuDeviceKind.HomeMaticIp];
        A.CallTo(() => ipApi.InitAsync(A<string>._, A<string>.That.StartsWith("hmc-")))
            .Returns(neverCompletes.Task);
        A.CallTo(() => ipApi.InitAsync(A<string>._, string.Empty))
            .ThrowsAsync(new HttpRequestException("Connection reset"));
        var sut = context.CreateSut();
        await sut.SubscribeAsync(CallbackUrl).WaitAsync(TimeSpan.FromSeconds(10));
        await sut.UnsubscribeAsync().WaitAsync(TimeSpan.FromSeconds(10));

        // Act
        var failures = await sut.UnsubscribeAsync();

        // Assert
        failures.Should().BeEmpty();
        A.CallTo(() => ipApi.InitAsync(A<string>._, string.Empty)).MustHaveHappenedOnceExactly();
    }

    private sealed class ApiContext
    {
        private CcuDeviceKind _currentKind;

        public ApiContext()
        {
            Apis = SubscribedKinds.ToDictionary(x => x, _ => A.Fake<IHomeMaticXmlRpcApi>());

            Builder = A.Fake<IHomeMaticXmlRpcApiBuilder>();
            A.CallTo(() => Builder.ForUrl(A<XmlRpcApiAddress>._))
                .ReturnsLazily((XmlRpcApiAddress address) =>
                {
                    address.BaseUrl.Should().Be(CcuUrl);
                    _currentKind = address.DeviceKind;
                    RequestedUrls.Add(address.ToApiUrl());
                    return Builder;
                });
            A.CallTo(() => Builder.Build()).ReturnsLazily(() => Apis[_currentKind]);
        }

        public IHomeMaticXmlRpcApiBuilder Builder { get; }

        public Dictionary<CcuDeviceKind, IHomeMaticXmlRpcApi> Apis { get; }

        public List<Uri> RequestedUrls { get; } = [];

        public CcuEventSubscriptions CreateSut()
        {
            return new CcuEventSubscriptions(Builder, CcuUrl, SubscribeTimeout, UnsubscribeTimeout);
        }
    }
}
