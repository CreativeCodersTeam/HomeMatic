using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class LinkFlagsTests
{
    [Fact]
    public void BitwiseOr_SenderAndReceiverBroken_ReportsBothFlagsAndBitmaskThree()
    {
        // Arrange & Act
        var flags = LinkFlags.SenderBroken | LinkFlags.ReceiverBroken;

        // Assert
        flags.HasFlag(LinkFlags.SenderBroken).Should().BeTrue();
        flags.HasFlag(LinkFlags.ReceiverBroken).Should().BeTrue();
        ((int) flags).Should().Be(3);
    }
}
