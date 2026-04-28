using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using CreativeCoders.Net.XmlRpc.Model.Values;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Converters;

public class FlagsMemberValueConverterLinkFlagsTests
{
    private readonly FlagsMemberValueConverter<LinkFlags> _sut = new();

    [Theory]
    [InlineData(0, LinkFlags.None)]
    [InlineData(1, LinkFlags.SenderBroken)]
    [InlineData(2, LinkFlags.ReceiverBroken)]
    [InlineData(3, LinkFlags.SenderBroken | LinkFlags.ReceiverBroken)]
    public void ConvertFromValue_IntegerValue_ReturnsLinkFlags(int raw, LinkFlags expected)
    {
        var result = _sut.ConvertFromValue(new IntegerValue(raw));

        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromValue_NonIntegerValue_ReturnsRawData()
    {
        var stringValue = new StringValue("hello");

        var result = _sut.ConvertFromValue(stringValue);

        result.Should().Be(stringValue.Data);
    }
}
