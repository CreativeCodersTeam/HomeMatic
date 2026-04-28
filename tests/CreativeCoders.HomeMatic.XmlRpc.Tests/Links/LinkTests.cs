using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class LinkTests
{
    [Fact]
    public void DefaultInstance_HasEmptyParamSets_AndNoNullProperties()
    {
        var link = new Link();

        link.Sender.Should().BeEmpty();
        link.Receiver.Should().BeEmpty();
        link.Name.Should().BeEmpty();
        link.Description.Should().BeEmpty();
        link.Flags.Should().Be(LinkFlags.None);
        link.SenderParamSet.Should().NotBeNull().And.BeEmpty();
        link.ReceiverParamSet.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ParamSets_AreMutable()
    {
        var link = new Link();

        link.SenderParamSet["TEMPERATURE"] = 21.5;
        link.ReceiverParamSet["LEVEL"] = 0.7;

        link.SenderParamSet.Should().ContainKey("TEMPERATURE").WhoseValue.Should().Be(21.5);
        link.ReceiverParamSet.Should().ContainKey("LEVEL").WhoseValue.Should().Be(0.7);
    }
}
