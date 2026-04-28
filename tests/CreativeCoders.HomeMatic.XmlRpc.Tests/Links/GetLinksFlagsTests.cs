using CreativeCoders.HomeMatic.XmlRpc.Links;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Links;

public class GetLinksFlagsTests
{
    [Theory]
    [InlineData(GetLinksFlags.None, 0)]
    [InlineData(GetLinksFlags.Group, 1)]
    [InlineData(GetLinksFlags.SenderParamSet, 2)]
    [InlineData(GetLinksFlags.ReceiverParamSet, 4)]
    public void EnumValue_MatchesSpecBitmask(GetLinksFlags flag, int expected)
    {
        ((int) flag).Should().Be(expected);
    }

    [Fact]
    public void AllFlags_Combined_EqualsSeven()
    {
        var combined = GetLinksFlags.Group | GetLinksFlags.SenderParamSet | GetLinksFlags.ReceiverParamSet;

        ((int) combined).Should().Be(7);
        combined.HasFlag(GetLinksFlags.Group).Should().BeTrue();
        combined.HasFlag(GetLinksFlags.SenderParamSet).Should().BeTrue();
        combined.HasFlag(GetLinksFlags.ReceiverParamSet).Should().BeTrue();
    }
}
