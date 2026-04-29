using System.Collections.Generic;
using CreativeCoders.HomeMatic.XmlRpc.Converters;
using CreativeCoders.Net.XmlRpc.Model;
using CreativeCoders.Net.XmlRpc.Model.Values;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.XmlRpc.Tests.Converters;

public class ParamSetDictionaryValueConverterTests
{
    [Fact]
    public void ConvertFromValue_StringValue_ReturnsEmptyDictionary()
    {
        var sut = new ParamSetDictionaryValueConverter();

        var result = sut.ConvertFromValue(new StringValue(string.Empty));

        result.Should().BeOfType<Dictionary<string, object>>().Which.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFromValue_NonEmptyStringValue_ReturnsEmptyDictionary()
    {
        var sut = new ParamSetDictionaryValueConverter();

        var result = sut.ConvertFromValue(new StringValue("unexpected"));

        result.Should().BeOfType<Dictionary<string, object>>().Which.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFromValue_IntegerValue_ReturnsEmptyDictionary()
    {
        var sut = new ParamSetDictionaryValueConverter();

        var result = sut.ConvertFromValue(new IntegerValue(0));

        result.Should().BeOfType<Dictionary<string, object>>().Which.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFromValue_EmptyStruct_ReturnsEmptyDictionary()
    {
        var sut = new ParamSetDictionaryValueConverter();

        var result = sut.ConvertFromValue(new StructValue(new Dictionary<string, XmlRpcValue>()));

        result.Should().BeOfType<Dictionary<string, object>>().Which.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFromValue_PopulatedStruct_ReturnsDictionaryWithMembers()
    {
        var sut = new ParamSetDictionaryValueConverter();
        var members = new Dictionary<string, XmlRpcValue>
        {
            ["NAME"] = new StringValue("test"),
            ["VALUE"] = new IntegerValue(42),
            ["ENABLED"] = new BooleanValue(true)
        };

        var result = sut.ConvertFromValue(new StructValue(members));

        result.Should().BeOfType<Dictionary<string, object>>()
            .Which.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["NAME"] = "test",
                ["VALUE"] = 42,
                ["ENABLED"] = true
            });
    }

    [Fact]
    public void ConvertFromObject_AnyValue_ThrowsNotImplementedException()
    {
        var sut = new ParamSetDictionaryValueConverter();

        var act = () => sut.ConvertFromObject(new Dictionary<string, object>());

        act.Should().Throw<NotImplementedException>();
    }
}
