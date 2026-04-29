using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.HomeMatic.XmlRpc.Parameters;
using CreativeCoders.Net.XmlRpc.Model.Values;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Converters;

public class FlagsMemberValueConverterRxModesTests
{
    [Theory]
    [InlineData(0, RxModes.None)]
    [InlineData(1, RxModes.Always)]
    [InlineData(2, RxModes.Burst)]
    [InlineData(10, RxModes.Burst | RxModes.WakeUp)]
    [InlineData(31, RxModes.Always | RxModes.Burst | RxModes.Config | RxModes.WakeUp | RxModes.LazyConfig)]
    public void ConvertFromValue_IntegerValue_ReturnsRxModes(int raw, RxModes expected)
    {
        // Arrange
        var sut = new FlagsMemberValueConverter<RxModes>();

        // Act
        var result = sut.ConvertFromValue(new IntegerValue(raw));

        // Assert
        result.Should().Be(expected);
    }
}
