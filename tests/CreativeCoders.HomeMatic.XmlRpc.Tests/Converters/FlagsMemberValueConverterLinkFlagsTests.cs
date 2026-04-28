using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using CreativeCoders.Net.XmlRpc.Model.Values;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Converters;

public class FlagsMemberValueConverterLinkFlagsTests
{
    [Theory]
    [InlineData(0, LinkFlags.None)]
    [InlineData(1, LinkFlags.SenderBroken)]
    [InlineData(2, LinkFlags.ReceiverBroken)]
    [InlineData(3, LinkFlags.SenderBroken | LinkFlags.ReceiverBroken)]
    public void ConvertFromValue_IntegerValue_ReturnsLinkFlags(int raw, LinkFlags expected)
    {
        // Arrange
        var sut = new FlagsMemberValueConverter<LinkFlags>();

        // Act
        var result = sut.ConvertFromValue(new IntegerValue(raw));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromValue_NonIntegerValue_ReturnsRawData()
    {
        // Arrange
        var sut = new FlagsMemberValueConverter<LinkFlags>();
        var stringValue = new StringValue("hello");

        // Act
        var result = sut.ConvertFromValue(stringValue);

        // Assert
        result.Should().Be(stringValue.Data);
    }

    [Fact]
    public void ConvertFromObject_AnyValue_ThrowsNotImplementedException()
    {
        // Arrange
        var sut = new FlagsMemberValueConverter<LinkFlags>();

        // Act
        Action act = () => sut.ConvertFromObject(LinkFlags.SenderBroken);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
