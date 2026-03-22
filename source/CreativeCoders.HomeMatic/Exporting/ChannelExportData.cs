namespace CreativeCoders.HomeMatic.Exporting;

public class ChannelExportData
{
    public required string Address { get; init; }

    public required string DeviceType { get; init; }

    public required int Index { get; init; }

    public required string[] ParamSets { get; init; }

    public required IEnumerable<ParamSetExportData> ParamSetValues { get; init; }
}
