using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class GetLinksFlagsTests
{
    [Fact]
    public void BitwiseOr_AllFlags_ProducesBitmaskSevenAndContainsEachFlag()
    {
        // Arrange & Act
        var combined = GetLinksFlags.Group | GetLinksFlags.SenderParamSet | GetLinksFlags.ReceiverParamSet;

        // Assert
        ((int) combined).Should().Be(7);
        combined.HasFlag(GetLinksFlags.Group).Should().BeTrue();
        combined.HasFlag(GetLinksFlags.SenderParamSet).Should().BeTrue();
        combined.HasFlag(GetLinksFlags.ReceiverParamSet).Should().BeTrue();
    }
}
