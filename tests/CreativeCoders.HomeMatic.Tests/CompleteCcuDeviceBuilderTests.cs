using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.XmlRpc.Exceptions;
using CreativeCoders.HomeMatic.XmlRpc.Links;
using CreativeCoders.Net.XmlRpc.Exceptions;
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
    public async Task BuildAsync_WhenValueHasNoMatchingDescription_ReturnsValueWithNullDescription()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        // Values contain "A", but the description list is empty -> Description stays null.
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
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        var paramSetValue = completeDevice.ParamSetValues.Single().ParamSetValues.Single();
        paramSetValue.ParamSetValue.Name.Should().Be("A");
        paramSetValue.Description.Should().BeNull();
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

    [Fact]
    public async Task BuildAsync_WithoutOptions_DoesNotFetchLinks()
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
        A.CallTo(channel)
            .Where(call => call.Method.Name == nameof(ICcuDeviceChannel.GetLinksAsync))
            .MustNotHaveHappened();
        completeDevice.Channels.Single().Links.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildAsync_WithIncludeLinks_FetchesLinksWithRequestedFlags()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns([]);
        A.CallTo(() => channel.ParamSets).Returns([]);

        var expectedLink = new Link { Sender = "X:1", Receiver = "Y:1", Name = "n", Description = "d" };
        const GetLinksFlags expectedFlags = GetLinksFlags.SenderParamSet;
        A.CallTo(() => channel.GetLinksAsync(expectedFlags))
            .Returns(Task.FromResult<IEnumerable<Link>>([expectedLink]));

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            IncludeLinks = true,
            LinksFlags = expectedFlags
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert
        A.CallTo(() => channel.GetLinksAsync(expectedFlags)).MustHaveHappenedOnceExactly();
        completeDevice.Channels.Single().Links.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(expectedLink);
    }

    [Fact]
    public async Task BuildAsync_WithIncludeLinksFalse_DoesNotFetchLinks()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns([]);
        A.CallTo(() => channel.ParamSets).Returns([]);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions { IncludeLinks = false };

        // Act
        await builder.BuildAsync(device, options);

        // Assert
        A.CallTo(channel)
            .Where(call => call.Method.Name == nameof(ICcuDeviceChannel.GetLinksAsync))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task BuildAsync_WhenParamSetValuesFault_ReturnsParamSetWithReadErrorInsteadOfThrowing()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "SERVICE"]);

        SetupParamSet(device, "MASTER", "AES_ACTIVE", true);

        A.CallTo(() => device.GetParamSetDescriptionsAsync("SERVICE"))
            .Returns(Task.FromResult(new CcuParameterDescriptions
            {
                ParamSetKey = "SERVICE",
                Items = []
            }));
        A.CallTo(() => device.GetParamSetValuesAsync("SERVICE"))
            .Throws(new FaultException(-321, string.Empty));

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        var paramSets = completeDevice.ParamSetValues.ToList();
        paramSets.Should().HaveCount(2);

        var masterParamSet = paramSets.Single(x => x.ParamSetKey == "MASTER");
        masterParamSet.ReadError.Should().BeNull();
        masterParamSet.ParamSetValues.Should().ContainSingle();

        var serviceParamSet = paramSets.Single(x => x.ParamSetKey == "SERVICE");
        serviceParamSet.ReadError.Should()
            .Be("XML-RPC fault -321 (device not reachable (e.g. sleeping battery-powered device))");
        serviceParamSet.ParamSetValues.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildAsync_WhenParamSetDescriptionsFault_ReturnsParamSetWithReadError()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        A.CallTo(() => device.GetParamSetDescriptionsAsync("MASTER"))
            .Throws(new GeneralException("General exception",
                new FaultException(-1, "Generic error (TRANSACTION_DISCARDED_FOR_UNREACHABLE_DEVICE)")));

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        var paramSet = completeDevice.ParamSetValues.Single();
        paramSet.ReadError.Should()
            .Be("XML-RPC fault -1 (General exception): Generic error (TRANSACTION_DISCARDED_FOR_UNREACHABLE_DEVICE)");
        paramSet.ParamSetValues.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildAsync_WhenChannelParamSetFaults_ReturnsChannelParamSetWithReadError()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns([]);
        A.CallTo(() => channel.ParamSets).Returns(["VALUES"]);

        A.CallTo(() => channel.GetParamSetDescriptionsAsync("VALUES"))
            .Throws(new FaultException(-9999, string.Empty));

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert - unknown fault code is rendered without a speaking description.
        var paramSet = completeDevice.Channels.Single().ParamSetValues.Single();
        paramSet.ReadError.Should().Be("XML-RPC fault -9999");
        paramSet.ParamSetValues.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildAsync_WithParamSetWhitelist_FetchesOnlyWhitelistedParamSets()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "SERVICE"]);
        A.CallTo(() => channel.ParamSets).Returns(["MASTER", "VALUES", "SERVICE"]);

        SetupParamSet(device, "MASTER", "A", 1);
        SetupParamSet(channel, "MASTER", "B", 2);
        SetupParamSet(channel, "VALUES", "C", 3);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            ParamSetWhitelist = ["MASTER", "VALUES"]
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert - SERVICE must not be requested at all.
        A.CallTo(() => device.GetParamSetValuesAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => device.GetParamSetDescriptionsAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => channel.GetParamSetValuesAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => channel.GetParamSetDescriptionsAsync("SERVICE")).MustNotHaveHappened();

        completeDevice.ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER");
        completeDevice.Channels.Single().ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER", "VALUES");
    }

    [Fact]
    public async Task BuildAsync_WithSkipServiceParamSet_DoesNotFetchServiceParamSet()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();
        var channel = A.Fake<ICcuDeviceChannel>();

        A.CallTo(() => device.Channels).Returns([channel]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "SERVICE"]);
        A.CallTo(() => channel.ParamSets).Returns(["MASTER", "VALUES", "SERVICE"]);

        SetupParamSet(device, "MASTER", "A", 1);
        SetupParamSet(channel, "MASTER", "B", 2);
        SetupParamSet(channel, "VALUES", "C", 3);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = true
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert - SERVICE must not be requested at all, neither on the device nor on the channel.
        A.CallTo(() => device.GetParamSetValuesAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => device.GetParamSetDescriptionsAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => channel.GetParamSetValuesAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => channel.GetParamSetDescriptionsAsync("SERVICE")).MustNotHaveHappened();

        completeDevice.ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER");
        completeDevice.Channels.Single().ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER", "VALUES");
    }

    [Fact]
    public async Task BuildAsync_WithSkipServiceParamSetAndWhitelistContainingService_DoesNotFetchServiceParamSet()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "SERVICE"]);

        SetupParamSet(device, "MASTER", "A", 1);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = true,
            ParamSetWhitelist = ["MASTER", "SERVICE"]
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert - skipping wins over an explicit whitelist entry.
        A.CallTo(() => device.GetParamSetValuesAsync("SERVICE")).MustNotHaveHappened();
        A.CallTo(() => device.GetParamSetDescriptionsAsync("SERVICE")).MustNotHaveHappened();

        completeDevice.ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER");
    }

    [Fact]
    public async Task BuildAsync_WithoutSkipServiceParamSet_FetchesServiceParamSet()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "SERVICE"]);

        SetupParamSet(device, "MASTER", "A", 1);
        SetupParamSet(device, "SERVICE", "ERROR_CODE", 0);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            SkipServiceParamSet = false
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert
        A.CallTo(() => device.GetParamSetValuesAsync("SERVICE")).MustHaveHappenedOnceExactly();

        completeDevice.ParamSetValues.Select(x => x.ParamSetKey)
            .Should().BeEquivalentTo("MASTER", "SERVICE");
    }

    [Fact]
    public async Task BuildAsync_WhenParamSetReadThrowsNonFaultException_PropagatesException()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        A.CallTo(() => device.GetParamSetDescriptionsAsync("MASTER"))
            .Throws(new InvalidOperationException("connection lost"));

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var act = async () => await builder.BuildAsync(device);

        // Assert - only FaultException is converted to ReadError, everything else propagates.
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task BuildAsync_WhitelistContainingLink_StillExcludesLinkParamSet()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER", "LINK"]);

        SetupParamSet(device, "MASTER", "A", 1);

        var builder = new CompleteCcuDeviceBuilder();
        var options = new CompleteCcuDeviceBuildOptions
        {
            ParamSetWhitelist = ["MASTER", "LINK"]
        };

        // Act
        var completeDevice = await builder.BuildAsync(device, options);

        // Assert
        A.CallTo(() => device.GetParamSetValuesAsync("LINK")).MustNotHaveHappened();
        A.CallTo(() => device.GetParamSetDescriptionsAsync("LINK")).MustNotHaveHappened();
        completeDevice.ParamSetValues.Select(x => x.ParamSetKey).Should().BeEquivalentTo("MASTER");
    }

    public static TheoryData<Exception, string> FaultCases => new()
    {
        {
            new GeneralException("General exception",
                new FaultException(-1, "Generic error (TRANSACTION_DISCARDED_FOR_UNREACHABLE_DEVICE)")),
            "XML-RPC fault -1 (General exception): Generic error (TRANSACTION_DISCARDED_FOR_UNREACHABLE_DEVICE)"
        },
        {
            new UnknownDeviceOrChannelException("Device or channel unknown", new FaultException(-2, string.Empty)),
            "XML-RPC fault -2 (Device or channel unknown)"
        },
        {
            new UnknownParamSetException("ParamSet unknown", new FaultException(-3, string.Empty)),
            "XML-RPC fault -3 (ParamSet unknown)"
        },
        {
            new FaultException(-321, string.Empty),
            "XML-RPC fault -321 (device not reachable (e.g. sleeping battery-powered device))"
        },
        { new FaultException(-9999, string.Empty), "XML-RPC fault -9999" },
        { new FaultException(-42, "boom"), "XML-RPC fault -42: boom" }
    };

    [Theory]
    [MemberData(nameof(FaultCases))]
    public async Task BuildAsync_WhenParamSetFaults_MapsFaultCodeToReadError(Exception thrownException,
        string expectedReadError)
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        A.CallTo(() => device.GetParamSetDescriptionsAsync("MASTER"))
            .Throws(thrownException);

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        completeDevice.ParamSetValues.Single().ReadError.Should().Be(expectedReadError);
    }

    [Fact]
    public async Task BuildAsync_WithPartiallyMatchingDescriptions_AssignsDescriptionsPerValue()
    {
        // Arrange
        var device = A.Fake<ICcuDevice>();

        A.CallTo(() => device.Channels).Returns([]);
        A.CallTo(() => device.ParamSets).Returns(["MASTER"]);

        A.CallTo(() => device.GetParamSetValuesAsync("MASTER"))
            .Returns(Task.FromResult<IEnumerable<ParamSetValue>>(
            [
                new ParamSetValue { Name = "DESCRIBED", Value = 1 },
                new ParamSetValue { Name = "UNDESCRIBED", Value = 2 }
            ]));

        A.CallTo(() => device.GetParamSetDescriptionsAsync("MASTER"))
            .Returns(Task.FromResult(new CcuParameterDescriptions
            {
                ParamSetKey = "MASTER",
                Items =
                [
                    new CcuParameterDescription
                    {
                        Id = "DESCRIBED",
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

        var builder = new CompleteCcuDeviceBuilder();

        // Act
        var completeDevice = await builder.BuildAsync(device);

        // Assert
        var values = completeDevice.ParamSetValues.Single().ParamSetValues.ToList();
        values.Should().HaveCount(2);
        values.Single(x => x.ParamSetValue.Name == "DESCRIBED").Description.Should().NotBeNull();
        values.Single(x => x.ParamSetValue.Name == "UNDESCRIBED").Description.Should().BeNull();
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
