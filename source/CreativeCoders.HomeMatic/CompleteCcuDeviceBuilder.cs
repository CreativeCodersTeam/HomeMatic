using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.Core.Devices;
using CreativeCoders.HomeMatic.Core.Parameters;
using CreativeCoders.HomeMatic.XmlRpc.Exceptions;
using CreativeCoders.Net.XmlRpc.Exceptions;

namespace CreativeCoders.HomeMatic;

/// <inheritdoc />
/// <summary>
/// Default implementation of <see cref="T:CreativeCoders.HomeMatic.Core.ICompleteCcuDeviceBuilder">ICompleteCcuDeviceBuilder</see> that augments an <see cref="T:CreativeCoders.HomeMatic.Core.Devices.ICcuDevice">ICcuDevice</see>
/// with the parameter-set values and descriptions of its device and channels.
/// </summary>
public class CompleteCcuDeviceBuilder : ICompleteCcuDeviceBuilder
{
    private const int FaultCodeDeviceNotReachable = -321;

    /// <inheritdoc />
    public async Task<ICompleteCcuDevice> BuildAsync(ICcuDevice device, CompleteCcuDeviceBuildOptions? options = null)
    {
        var channels = await GetChannelsAsync(device, options).ConfigureAwait(false);

        var completeDevice = new CompleteCcuDevice
        {
            DeviceData = device,
            Channels = channels,
            ParamSetValues = await GetParamSetValuesAsync(device, options).ConfigureAwait(false)
        };

        return completeDevice;
    }

    private static async Task<IEnumerable<ICompleteCcuDeviceChannel>> GetChannelsAsync(ICcuDevice device,
        CompleteCcuDeviceBuildOptions? options)
    {
        var channels = new List<ICompleteCcuDeviceChannel>();

        foreach (var ccuDeviceChannel in device.Channels)
        {
            var links = options?.IncludeLinks == true
                ? (await ccuDeviceChannel.GetLinksAsync(options.LinksFlags).ConfigureAwait(false)).ToArray()
                : [];

            var completeChannel = new CompleteCcuDeviceChannel
            {
                ChannelData = ccuDeviceChannel,
                ParamSetValues = await GetParamSetValuesAsync(ccuDeviceChannel, options).ConfigureAwait(false),
                Links = links
            };

            channels.Add(completeChannel);
        }

        return [..channels];
    }

    private static async Task<IEnumerable<ParamSetValuesWithDescriptions>> GetParamSetValuesAsync(
        ICcuDeviceBase device, CompleteCcuDeviceBuildOptions? options)
    {
        var paramSetValues = new List<ParamSetValuesWithDescriptions>();

        var paramSetKeys = device.ParamSets
            .Where(x => x != ParamSetKey.Link && (options?.IsParamSetAllowed(x) ?? true));

        foreach (var paramSetKey in paramSetKeys)
        {
            try
            {
                var descriptions = await device.GetParamSetDescriptionsAsync(paramSetKey).ConfigureAwait(false);

                var descriptionsById = descriptions.Items
                    .Where(x => x.Id is not null)
                    .DistinctBy(x => x.Id)
                    .ToDictionary(x => x.Id!);

                var paramSets = (await device.GetParamSetValuesAsync(paramSetKey).ConfigureAwait(false))
                    .Select(x => new ParamSetValueWithDescription
                    {
                        ParamSetValue = x,
                        Description = descriptionsById.GetValueOrDefault(x.Name)
                    })
                    .ToList();

                paramSetValues.Add(new ParamSetValuesWithDescriptions
                {
                    ParamSetKey = paramSetKey,
                    ParamSetValues = paramSets
                });
            }
            catch (Exception exception) when (exception is FaultException or CcuXmlRpcException)
            {
                paramSetValues.Add(new ParamSetValuesWithDescriptions
                {
                    ParamSetKey = paramSetKey,
                    ParamSetValues = [],
                    ReadError = BuildReadErrorMessage(exception)
                });
            }
        }

        return [..paramSetValues];
    }

    private static string BuildReadErrorMessage(Exception exception)
    {
        // Fault codes -1..-10 arrive as typed CcuXmlRpcException with a speaking message and the
        // original FaultException as inner exception; unmapped codes (e.g. -321) arrive raw.
        if (exception is CcuXmlRpcException ccuXmlRpcException)
        {
            return ccuXmlRpcException.InnerException is FaultException innerFaultException
                ? FormatReadError(innerFaultException, ccuXmlRpcException.Message)
                : ccuXmlRpcException.Message;
        }

        var faultException = (FaultException)exception;

        var faultDescription = faultException.FaultCode == FaultCodeDeviceNotReachable
            ? "device not reachable (e.g. sleeping battery-powered device)"
            : null;

        return FormatReadError(faultException, faultDescription);
    }

    private static string FormatReadError(FaultException faultException, string? description)
    {
        var message = description is null
            ? $"XML-RPC fault {faultException.FaultCode}"
            : $"XML-RPC fault {faultException.FaultCode} ({description})";

        return string.IsNullOrEmpty(faultException.FaultMessage)
            ? message
            : $"{message}: {faultException.FaultMessage}";
    }
}
