using CreativeCoders.HomeMatic.Abstractions;
using CreativeCoders.HomeMatic.Abstractions.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;

namespace CreativeCoders.HomeMatic;

public class CompleteCcuDeviceBuilder : ICompleteCcuDeviceBuilder
{
    public async Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device)
    {
        var channels = await GetChannelsAsync(device).ConfigureAwait(false);

        var completeDevice = new CompleteCcuDevice
        {
            DeviceData = device,
            Channels = channels
        };

        return completeDevice;
    }

    private async Task<IEnumerable<ICompleteCcuDeviceChannel>> GetChannelsAsync(ICcuDevice device)
    {
        var channels = new List<ICompleteCcuDeviceChannel>();

        foreach (var ccuDeviceChannel in device.Channels)
        {
            var completeChannel = new CompleteCcuDeviceChannel
            {
                ChannelData = ccuDeviceChannel,
                ParamSetValues = await GetParamSetValuesAsync(ccuDeviceChannel).ConfigureAwait(false)
            };

            channels.Add(completeChannel);
        }

        return [..channels];
    }

    private async Task<IEnumerable<ParamSetValuesWithDescriptions>> GetParamSetValuesAsync(ICcuDeviceBase device)
    {
        var paramSetValues = new List<ParamSetValuesWithDescriptions>();

        foreach (var paramSetKey in device.ParamSets.Where(x => x != ParamSetKey.Link))
        {
            var descriptions = await device.GetParamSetDescriptionsAsync(paramSetKey).ConfigureAwait(false);

            var paramSets = (await device.GetParamSetValuesAsync(paramSetKey).ConfigureAwait(false))
                .Select(x => new ParamSetValueWithDescription
                {
                    ParamSetValue = x,
                    Description = descriptions.Items.FirstOrDefault(y => y.Id == x.Name) ??
                                  throw new KeyNotFoundException()
                });

            paramSetValues.Add(new ParamSetValuesWithDescriptions()
            {
                ParamSetKey = paramSetKey,
                ParamSetValues = paramSets
            });
        }

        return [..paramSetValues];
    }
}

public class CompleteCcuDeviceChannel : ICompleteCcuDeviceChannel
{
    public required ICcuDeviceChannelData ChannelData { get; init; }

    public required IEnumerable<ParamSetValuesWithDescriptions> ParamSetValues { get; init; }
}

public class CompleteCcuDevice : ICompleteCcuDevice
{
    public required ICcuDeviceData DeviceData { get; init; }

    public required IEnumerable<ICompleteCcuDeviceChannel> Channels { get; init; }
}
