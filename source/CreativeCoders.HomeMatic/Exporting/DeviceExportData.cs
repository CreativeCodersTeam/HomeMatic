namespace CreativeCoders.HomeMatic.Exporting;

public class DeviceExportData
{
    public required string Name { get; init; }

    public required string Address { get; init; }

    public required string DeviceType { get; init; }

    public required string[] ParamSetKeys { get; init; }

    public required string FirmwareVersion { get; init; }

    public required string Ccu { get; init; }

    public required IEnumerable<ParamSetExportData> ParamSetValues { get; init; }

    public required IEnumerable<ChannelExportData> Channels { get; init; }
}
