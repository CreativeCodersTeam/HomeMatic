using CreativeCoders.HomeMatic.Core;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CcuRoutingTableTests
{
    [Fact]
    public void TryGetClient_UnknownAddress_ReturnsFalse()
    {
        var table = new CcuRoutingTable();

        var found = table.TryGetClient("ABC0001234", out var client);

        found.Should().BeFalse();
        client.Should().BeNull();
    }

    [Fact]
    public void Register_ThenTryGetClient_ReturnsRegisteredClient()
    {
        var table = new CcuRoutingTable();
        var ccuClient = A.Fake<ICcuClient>();

        table.Register("ABC0001234", ccuClient);

        table.TryGetClient("ABC0001234", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(ccuClient);
    }

    [Fact]
    public void Register_SameAddressTwice_OverwritesExistingEntry()
    {
        var table = new CcuRoutingTable();
        var firstClient = A.Fake<ICcuClient>();
        var secondClient = A.Fake<ICcuClient>();

        table.Register("ABC0001234", firstClient);
        table.Register("ABC0001234", secondClient);

        table.TryGetClient("ABC0001234", out var resolved).Should().BeTrue();
        resolved.Should().BeSameAs(secondClient);
    }

    [Fact]
    public void RegisterBulk_AddsAllEntries()
    {
        var table = new CcuRoutingTable();
        var clientA = A.Fake<ICcuClient>();
        var clientB = A.Fake<ICcuClient>();

        table.Register(
        [
            new KeyValuePair<string, ICcuClient>("A", clientA),
            new KeyValuePair<string, ICcuClient>("B", clientB)
        ]);

        table.TryGetClient("A", out var resolvedA).Should().BeTrue();
        resolvedA.Should().BeSameAs(clientA);

        table.TryGetClient("B", out var resolvedB).Should().BeTrue();
        resolvedB.Should().BeSameAs(clientB);
    }

    [Fact]
    public void Invalidate_RemovesEntry()
    {
        var table = new CcuRoutingTable();
        var ccuClient = A.Fake<ICcuClient>();
        table.Register("ABC0001234", ccuClient);

        table.Invalidate("ABC0001234");

        table.TryGetClient("ABC0001234", out _).Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var table = new CcuRoutingTable();
        table.Register("A", A.Fake<ICcuClient>());
        table.Register("B", A.Fake<ICcuClient>());

        table.Clear();

        table.TryGetClient("A", out _).Should().BeFalse();
        table.TryGetClient("B", out _).Should().BeFalse();
    }
}
