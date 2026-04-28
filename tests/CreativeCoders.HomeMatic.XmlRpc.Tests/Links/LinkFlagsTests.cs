using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class LinkFlagsTests
{
    [Fact]
    public void None_HasIntegerValue_Zero()
    {
        ((int) LinkFlags.None).Should().Be(0);
    }

    [Fact]
    public void SenderBroken_HasIntegerValue_One()
    {
        ((int) LinkFlags.SenderBroken).Should().Be(1);
    }

    [Fact]
    public void ReceiverBroken_HasIntegerValue_Two()
    {
        ((int) LinkFlags.ReceiverBroken).Should().Be(2);
    }

    [Fact]
    public void CombinedFlags_AreReportedCorrectly()
    {
        var flags = LinkFlags.SenderBroken | LinkFlags.ReceiverBroken;

        flags.HasFlag(LinkFlags.SenderBroken).Should().BeTrue();
        flags.HasFlag(LinkFlags.ReceiverBroken).Should().BeTrue();
        ((int) flags).Should().Be(3);
    }
}
