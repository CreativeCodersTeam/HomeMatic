using System.Text.Json;
using System.Text.Json.Serialization;
using CreativeCoders.HomeMatic.Core.Devices;

namespace CreativeCoders.HomeMatic.Exporting;

public class DeviceExporter : IDeviceExporter
{
    public Task<string> ExportDeviceAsync(ICompleteCcuDevice device, DeviceExportOptions? options = null)
    {
        var exportData = BuildExportData(device, options);
        var json = Serialize(exportData, options);
        return Task.FromResult(json);
    }

    public Task<string> ExportDevicesAsync(IEnumerable<ICompleteCcuDevice> devices, DeviceExportOptions? options = null)
    {
        var exportDataList = devices.Select(d => BuildExportData(d, options)).ToList();
        var json = Serialize(exportDataList, options);
        return Task.FromResult(json);
    }

    public DeviceExportData BuildExportData(ICompleteCcuDevice device, DeviceExportOptions? options = null)
    {
        return new DeviceExportData
        {
            Name = device.DeviceData.Name,
            Address = device.DeviceData.Uri.Address,
            DeviceType = device.DeviceData.DeviceType,
            ParamSetKeys = device.DeviceData.ParamSets,
            FirmwareVersion = device.DeviceData.Firmware,
            Ccu = device.DeviceData.Uri.HostDisplayName,
            ParamSetValues = BuildParamSetExportData(device.ParamSetValues, options),
            Channels = device.Channels.Select(ch => BuildChannelExportData(ch, options)).ToList()
        };
    }

    private static ChannelExportData BuildChannelExportData(
        ICompleteCcuDeviceChannel channel,
        DeviceExportOptions? options)
    {
        return new ChannelExportData
        {
            Address = channel.ChannelData.Uri.Address,
            DeviceType = channel.ChannelData.DeviceType,
            Index = channel.ChannelData.Index,
            ParamSets = channel.ChannelData.ParamSets,
            ParamSetValues = BuildParamSetExportData(channel.ParamSetValues, options)
        };
    }

    private static IEnumerable<ParamSetExportData> BuildParamSetExportData(
        IEnumerable<ParamSetValuesWithDescriptions> paramSetValues,
        DeviceExportOptions? options)
    {
        return paramSetValues
            .Where(ps => options?.IsParamSetAllowed(ps.ParamSetKey) ?? true)
            .Select(ps => new ParamSetExportData
            {
                ParamSetKey = ps.ParamSetKey,
                Values = ps.ParamSetValues.Select(v => new ParamValueExportData
                {
                    Key = v.ParamSetValue.Name,
                    Name = v.Description.Id,
                    Value = v.ParamSetValue.Value
                }).ToList()
            })
            .ToList();
    }

    private static string Serialize<T>(T data, DeviceExportOptions? options)
    {
        var serializerOptions = CreateJsonSerializerOptions(options);
        return JsonSerializer.Serialize(data, serializerOptions);
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions(DeviceExportOptions? options)
    {
        return new JsonSerializerOptions
        {
            WriteIndented = options?.WriteIndented ?? true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
