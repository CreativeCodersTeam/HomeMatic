using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class LinkTests
{
    [Fact]
    public void Constructor_Default_InitialisesAllPropertiesToEmptyDefaults()
    {
        // Arrange & Act
        var link = new Link();

        // Assert
        link.Sender.Should().BeEmpty();
        link.Receiver.Should().BeEmpty();
        link.Name.Should().BeEmpty();
        link.Description.Should().BeEmpty();
        link.Flags.Should().Be(LinkFlags.None);
        link.SenderParamSet.Should().NotBeNull().And.BeEmpty();
        link.ReceiverParamSet.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ParamSets_WhenAssigned_AreMutable()
    {
        // Arrange
        var link = new Link();

        // Act
        link.SenderParamSet["TEMPERATURE"] = 21.5;
        link.ReceiverParamSet["LEVEL"] = 0.7;

        // Assert
        link.SenderParamSet.Should().ContainKey("TEMPERATURE").WhoseValue.Should().Be(21.5);
        link.ReceiverParamSet.Should().ContainKey("LEVEL").WhoseValue.Should().Be(0.7);
    }
}
