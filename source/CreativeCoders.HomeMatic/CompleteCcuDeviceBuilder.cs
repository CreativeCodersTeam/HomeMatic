using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Default implementation of <see cref="T:CreativeCoders.HomeMatic.Core.ICompleteCcuDeviceBuilder">ICompleteCcuDeviceBuilder</see> that augments an <see cref="T:CreativeCoders.HomeMatic.Core.Devices.ICcuDevice">ICcuDevice</see>
/// with the parameter-set values and descriptions of its device and channels.
/// </summary>
public class CompleteCcuDeviceBuilder : ICompleteCcuDeviceBuilder
{
    /// <inheritdoc />
    public async Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device)
    {
        var channels = await GetChannelsAsync(device).ConfigureAwait(false);

        var completeDevice = new CompleteCcuDevice
        {
            DeviceData = device,
            Channels = channels,
            ParamSetValues = await GetParamSetValuesAsync(device).ConfigureAwait(false)
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
