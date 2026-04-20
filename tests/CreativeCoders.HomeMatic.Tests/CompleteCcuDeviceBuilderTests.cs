using CreativeCoders.HomeMatic.Core.Devices;
using FakeItEasy;
using AwesomeAssertions;

namespace CreativeCoders.HomeMatic.Tests;

public class CompleteCcuDeviceBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithDeviceAndChannels_ReturnsCompleteDeviceWithParamSetValues()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);
        A.CallTo(() => channel.ParamSets).Returns(["VALUES"]);

        SetupParamSet(device, "MASTER", "AES_ACTIVE", true);
        SetupParamSet(channel, "VALUES", "STATE", false);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        completeDevice.DeviceData.Should().BeSameAs(device);

        var channels = completeDevice.Channels.ToList();
        channels.Should().HaveCount(1);
        channels[0].ChannelData.Should().BeSameAs(channel);

        var channelParamSets = channels[0].ParamSetValues.ToList();
        channelParamSets.Should().HaveCount(1);
        channelParamSets[0].ParamSetKey.Should().Be("VALUES");
        channelParamSets[0].ParamSetValues.Should().ContainSingle()
            .Which.ParamSetValue.Name.Should().Be("STATE");

        var deviceParamSets = completeDevice.ParamSetValues.ToList();
        deviceParamSets.Should().HaveCount(1);
        deviceParamSets[0].ParamSetKey.Should().Be("MASTER");
        deviceParamSets[0].ParamSetValues.Should().ContainSingle()
            .Which.ParamSetValue.Name.Should().Be("AES_ACTIVE");
    }

    [Fact]
    public async Task BuildAsync_SkipsLinkParamSetKey()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "LINK", "VALUES"]);

        SetupParamSet(device, "MASTER", "A", 1);
        SetupParamSet(device, "VALUES", "B", 2);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert - LINK must not be requested because it is filtered out.
        A.CallTo(() => device.GetParamSetValuesAsync("LINK")).MustNotHaveHappened();
        A.CallTo(() => device.GetParamSetDescriptionsAsync("LINK")).MustNotHaveHappened();

        completeDevice.ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER", "VALUES");
    }

    [Fact]
    public async Task BuildAsync_WhenValueHasNoMatchingDescription_ThrowsKeyNotFoundException()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        // Values contain "A", but the description list is empty -> FirstOrDefault returns null -> throw.
        A.CallTo(() => device.GetParamSetValuesAsync("MASTER"))
            .Returns(Task.FromResult<IEnumerable<ParamSetValue>>(
            [
                new ParamSetValue { Name = "A", Value = 1 }
            ]));

        A.CallTo(() => device.GetParamSetDescriptionsAsync("MASTER"))
            .Returns(Task.FromResult(new CcuParameterDescriptions
            {
                ParamSetKey = "MASTER",
                Items = []
            }));

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var act = async () =>
        {
            var result = await builder.BuildAsync(device);
            // ParamSetValues uses deferred execution via Select; force enumeration.
            foreach (var paramSet in result.ParamSetValues)
            {
                _ = paramSet.ParamSetValues.ToList();
            }
        };

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task BuildAsync_WithEmptyParamSets_ReturnsEmptyParamSetValues()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns([]);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        completeDevice.ParamSetValues.Should().BeEmpty();
        completeDevice.Channels.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildAsync_WithMultipleChannelsEachHavingOwnParamSets_MapsEachChannelIndependently()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channelA = A.Fake<ICcuDeviceChannel>();
        var channelB = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channelA, channelB]);
        A.CallTo(() => device.ParamSets).Returns([]);
        A.CallTo(() => channelA.ParamSets).Returns(["VALUES"]);
        A.CallTo(() => channelB.ParamSets).Returns(["MASTER"]);

        SetupParamSet(channelA, "VALUES", "LEVEL", 50);
        SetupParamSet(channelB, "MASTER", "AES_ACTIVE", true);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        var channels = completeDevice.Channels.ToList();
        channels.Should().HaveCount(2);
        channels[0].ChannelData.Should().BeSameAs(channelA);
        channels[0].ParamSetValues.Should().ContainSingle()
            .Which.ParamSetKey.Should().Be("VALUES");
        channels[1].ChannelData.Should().BeSameAs(channelB);
        channels[1].ParamSetValues.Should().ContainSingle()
            .Which.ParamSetKey.Should().Be("MASTER");
    }

    [Fact]
    public async Task BuildAsync_ChannelWithoutParamSets_ReturnsChannelWithEmptyParamSetValues()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns([]);
        A.CallTo(() => channel.ParamSets).Returns([]);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        completeDevice.Channels.Should().ContainSingle()
            .Which.ParamSetValues.Should().BeEmpty();
    }

    private static void SetupParamSet(ICcuDeviceBase device, string paramSetKey, string name, object value)
    {
        A.CallTo(() => device.GetParamSetValuesAsync(paramSetKey))
            .Returns(Task.FromResult<IEnumerable<ParamSetValue>>(
            [
                new ParamSetValue { Name = name, Value = value }
            ]));

        A.CallTo(() => device.GetParamSetDescriptionsAsync(paramSetKey))
            .Returns(Task.FromResult(new CcuParameterDescriptions
            {
                ParamSetKey = paramSetKey,
                Items =
                [
                    new CcuParameterDescription
                    {
                        Id = name,
                        DefaultValue = null,
                        MinValue = null,
                        MaxValue = null,
                        Type = null,
                        DataType = default,
                        Unit = null,
                        TabOrder = 0,
                        Control = null,
                        ValuesList = [],
                        SpecialValues = []
                    }
                ]
            }));
    }
}
