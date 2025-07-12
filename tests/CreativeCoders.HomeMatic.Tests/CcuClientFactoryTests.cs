using System.Net;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using FakeItEasy;
using FluentAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuClientFactoryTests
{
    [Fact]
    public void CreateClient_ShouldReturnCcuClient_WhenCalled()
    {
        // Arrange
        var xmlRpcApiBuilder = A.Fake<IHomeMaticXmlRpcApiBuilder>();
        var jsonRpcApiBuilder = A.Fake<IHomeMaticJsonRpcClientBuilder>();

        A.CallTo(() => jsonRpcApiBuilder.WithCredentials(A<NetworkCredential>._))
            .Returns(jsonRpcApiBuilder);

        var ccuClientFactory = new CcuClientFactory(xmlRpcApiBuilder, jsonRpcApiBuilder);

        // Act
        var ccuClient = ccuClientFactory.CreateClient([CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp],
            "example.com", "username", "password");

        // Assert
        A.CallTo(() => xmlRpcApiBuilder.ForUrl(new Uri($"http://example.com:{CcuRpcPorts.HomeMatic}")))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => xmlRpcApiBuilder.ForUrl(new Uri($"http://example.com:{CcuRpcPorts.HomeMaticIp}")))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => xmlRpcApiBuilder.ForUrl(new Uri($"http://example.com:{CcuRpcPorts.HomeMaticWired}")))
            .MustNotHaveHappened();

        A.CallTo(() => xmlRpcApiBuilder.ForUrl(new Uri($"http://example.com:{CcuRpcPorts.CoupledDevices}")))
            .MustNotHaveHappened();

        A.CallTo(() => jsonRpcApiBuilder.ForUrl(new Uri($"http://example.com")))
            .MustHaveHappenedOnceExactly();

        ccuClient
            .Should()
            .NotBeNull();
    }
}
