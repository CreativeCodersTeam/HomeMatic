using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class MultiCcuClientFactoryTests
{
    [Fact]
    public void AddCcu_CallsClientFactoryWithProvidedArguments()
    {
        // Arrange
        var ccuClientFactory = A.Fake<ICcuClientFactory>();
        var factory = new MultiCcuClientFactory(ccuClientFactory);
        var deviceKinds = new[] { CcuDeviceKind.HomeMatic, CcuDeviceKind.HomeMaticIp };

        // Act
        factory.AddCcu("ccu1", "example.com", "admin", "secret", deviceKinds);

        // Assert
        A.CallTo(() => ccuClientFactory.CreateClient("ccu1",
                A<IEnumerable<CcuDeviceKind>>.That.IsSameSequenceAs(deviceKinds),
                "example.com", "admin", "secret"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void AddCcu_ReturnsSameFactoryInstanceForFluentChaining()
    {
        // Arrange
        var ccuClientFactory = A.Fake<ICcuClientFactory>();
        var factory = new MultiCcuClientFactory(ccuClientFactory);

        // Act
        var result = factory.AddCcu("ccu1", "host", "user", "pwd", CcuDeviceKind.HomeMatic);

        // Assert
        result.Should().BeSameAs(factory);
    }

    [Fact]
    public void Build_WithoutAddedCcus_ReturnsMultiCcuClient()
    {
        // Arrange
        var ccuClientFactory = A.Fake<ICcuClientFactory>();
        var factory = new MultiCcuClientFactory(ccuClientFactory);

        // Act
        var client = factory.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<MultiCcuClient>();
    }

    [Fact]
    public void Build_AfterAddCcu_ReturnsMultiCcuClientConfiguredWithCreatedClients()
    {
        // Arrange
        var ccuClientFactory = A.Fake<ICcuClientFactory>();
        var createdClient = A.Fake<ICcuClient>();
        A.CallTo(() => ccuClientFactory.CreateClient(
                A<string>._, A<IEnumerable<CcuDeviceKind>>._, A<string>._, A<string>._, A<string>._))
            .Returns(createdClient);

        var factory = new MultiCcuClientFactory(ccuClientFactory)
            .AddCcu("ccu1", "host", "user", "pwd", CcuDeviceKind.HomeMatic);

        // Act
        var client = factory.Build();

        // Assert - AddCcu uses the factory; Build constructs a MultiCcuClient.
        client.Should().BeOfType<MultiCcuClient>();
        A.CallTo(() => ccuClientFactory.CreateClient(
                "ccu1", A<IEnumerable<CcuDeviceKind>>._, "host", "user", "pwd"))
            .MustHaveHappenedOnceExactly();
    }
}
